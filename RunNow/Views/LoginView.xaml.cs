using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json; // JObject 외에 직렬화 쓸 때 유용
using Newtonsoft.Json.Linq;
using RunNow.Services;
using RunNow.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;



namespace RunNow.Views
{
    public partial class LoginView : UserControl
    {
        private readonly LoginViewModel vm;

        public LoginView()
        {
            InitializeComponent();

            vm = App.Services.GetRequiredService<LoginViewModel>();
            DataContext = vm;

            // PasswordBox 값 ViewModel.Password에 반영
            PasswordBox.PasswordChanged += (s, e) =>
            {
                vm.Password = PasswordBox.Password; // ViewModel의 OnPasswordChanged에서 CanExecute 갱신됨
            };

            // 성공 시 내부 로직만 (UI는 여기서 절대 띄우지 않음)
            vm.FaceDetected += async () =>
            {
                await Task.CompletedTask;
            };
        }

        // 회원가입 클릭
        private void Register_Click(object sender, MouseButtonEventArgs e)
        {
            vm.NavigateRegisterCommand.Execute(null);
        }

        private void FindPassword_Click(object sender, MouseButtonEventArgs e)
        {
            vm.FindPasswordCommand.Execute(null);
        }


        private async void FaceRecognition_Click(object sender, RoutedEventArgs e)
        {
            ShowFacePopup("face_id.png", "얼굴 인식 중...");
            string status = await RunFaceRecognitionAsync();
            ShowUIForStatus(status);
        }


        private async Task<string> RunFaceRecognitionAsync()
        {
            try
            {
                string pythonPath = @"C:\Users\YESOM\PycharmProjects\PythonProject1\.venv\Scripts\python.exe";
                string scriptPath = @"C:\Users\YESOM\PycharmProjects\PythonProject1\real_check_Face.py";

                if (!File.Exists(pythonPath))
                {
                    FacePopup.Visibility = Visibility.Collapsed;
                    MessageBox.Show("Python 실행 파일을 찾을 수 없습니다.", "로그인", MessageBoxButton.OK, MessageBoxImage.Error);
                    return "PYTHON_NOT_FOUND";
                }

                if (!File.Exists(scriptPath))
                {
                    FacePopup.Visibility = Visibility.Collapsed;
                    MessageBox.Show("real_check_Face.py 파일을 찾을 수 없습니다.", "로그인", MessageBoxButton.OK, MessageBoxImage.Error);
                    return "SCRIPT_NOT_FOUND";
                }

                var psi = new ProcessStartInfo
                {
                    FileName = pythonPath,
                    Arguments = $"\"{scriptPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = psi };

                if (!process.Start())
                {
                    FacePopup.Visibility = Visibility.Collapsed;
                    MessageBox.Show("Python 프로세스를 시작하지 못했습니다.", "로그인", MessageBoxButton.OK, MessageBoxImage.Error);
                    return "ERROR";
                }

                // 파이썬 출력 수집
                Task<string> readStdOut = process.StandardOutput.ReadToEndAsync();
                Task<string> readStdErr = process.StandardError.ReadToEndAsync();

#if NET6_0_OR_GREATER
                await process.WaitForExitAsync().ConfigureAwait(false);
#else
        process.WaitForExit();
#endif

                string output = await readStdOut.ConfigureAwait(false);
                string error = await readStdErr.ConfigureAwait(false);

                Debug.WriteLine("[py][stdout]\n" + output);
                if (!string.IsNullOrWhiteSpace(error))
                    Debug.WriteLine("[py][stderr]\n" + error);

                // 1) 파이썬 stdout에서 임베딩/상태 JSON 추출 (마지막 의미있는 JSON 1개)
                JObject py = null;
                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var raw in lines)
                {
                    var line = raw.Trim();
                    if (!(line.StartsWith("{") && line.EndsWith("}"))) continue;
                    try
                    {
                        var obj = JObject.Parse(line);
                        if (obj["embedding"] != null || obj["status"] != null)
                            py = obj;
                    }
                    catch { /* skip */ }
                }

                if (py == null)
                {
                    FacePopup.Visibility = Visibility.Collapsed;
                    MessageBox.Show("얼굴 인식 결과(JSON)를 받지 못했습니다.", "로그인", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return "INVALID_JSON";
                }

                // 파이썬 단계 에러/중단 처리
                string pyStatus = py["status"]?.ToString().Trim().ToUpperInvariant();
                if (pyStatus == "CAMERA_ERROR" || pyStatus == "NO_FACE" || pyStatus == "ERROR")
                {
                    return pyStatus switch
                    {
                        "CAMERA_ERROR" => "CAMERA_ERROR",
                        "NO_FACE" => "NO_FACE",
                        _ => "ERROR"
                    };
                }

                // 임베딩 확보
                if (!(py.TryGetValue("embedding", out JToken embTok) && embTok is JArray embArr))
                {
                    FacePopup.Visibility = Visibility.Collapsed;
                    MessageBox.Show("임베딩 데이터가 없습니다.", "로그인", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return "INVALID_JSON";
                }

                float[] embedding;
                try { embedding = embArr.ToObject<float[]>(); }
                catch
                {
                    var d = embArr.ToObject<double[]>();
                    embedding = Array.ConvertAll(d, x => (float)x);
                }

                // 2) 서버 송수신: 너희 AuthService 사용
                var auth = App.Services.GetRequiredService<IAuthService>();
                JObject serverResult = await auth.FaceLoginAsync(embedding); // ← 여기서 TcpClientService 통해 송수신

                if (serverResult == null)
                {
                    FacePopup.Visibility = Visibility.Collapsed;
                    MessageBox.Show("서버 응답이 비어있습니다.", "로그인", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return "INVALID_JSON";
                }

                Debug.WriteLine("[server][resp] " + serverResult.ToString());

                // 3) 서버 응답 판정 (protocol 우선 → status 보조)
                string protocol = serverResult["protocol"]?.ToString().Trim();
                string status = serverResult["status"]?.ToString().Trim();

                string finalCode;
                if (string.Equals(protocol, "2_1", StringComparison.OrdinalIgnoreCase)) finalCode = "2_1";
                else if (string.Equals(protocol, "2_2", StringComparison.OrdinalIgnoreCase)) finalCode = "2_2";
                else if (string.Equals(status, "success", StringComparison.OrdinalIgnoreCase)) finalCode = "2_1";
                else if (string.Equals(status, "no_match", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(status, "fail", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(status, "error", StringComparison.OrdinalIgnoreCase)) finalCode = "2_2";
                else finalCode = "UNKNOWN";

                Debug.WriteLine($"[server] protocol={protocol}, status={status}, resolved={finalCode}");

                // 4) 성공 시 내부 이벤트/임베딩 저장
                if (finalCode == "2_1")
                {
                    vm.FaceEmbedding = embedding;
                    if (vm.FaceDetected != null)
                        await vm.FaceDetected.Invoke();
                }

                return finalCode;
            }
            catch (Exception ex)
            {
                FacePopup.Visibility = Visibility.Collapsed;
                MessageBox.Show("실행 오류: " + ex.Message, "로그인", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine("exception: " + ex);
                return "ERROR";
            }
        }




        /// <summary>
        /// 상태 코드 → UI 매핑
        /// 실패/에러는 MessageBox, 그 외(=정상)는 환영합니다 팝업.
        /// 반환값: 팝업을 사용했으면 true (그 외 false)
        /// </summary>
        private void ShowUIForStatus(string status)
        {
            var code = (status ?? "").Trim().ToUpperInvariant();
            Debug.WriteLine($"[ShowUIForStatus] raw='{status}' normalized='{code}'");

            // 성공 케이스
            if (code == "2_1" || code == "SUCCESS")
            {
                ShowFacePopup("check.png", "환영합니다");
                Task.Delay(2000).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() => FacePopup.Visibility = Visibility.Collapsed);
                });
                return;
            }

            // 실패 케이스(인증 불일치): 팝업으로 안내
            if (code == "2_2" || code == "FAIL" || code == "FAILED" || code == "NO_MATCH")
            {
                // 기존에 쓰던 팝업 UI 재사용 (안전하게 face_id.png 사용)
                ShowFacePopup("face_id.png", "등록되지 않은 얼굴입니다.");
                Task.Delay(2000).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() => FacePopup.Visibility = Visibility.Collapsed);
                });
                return;
            }

            // 그 외 오류/안내는 MessageBox 유지
            FacePopup.Visibility = Visibility.Collapsed;

            string msg = code switch
            {
                "NO_FACE" => "등록된 얼굴이 없습니다.",
                "CAMERA_ERROR" => "카메라 오류가 발생했습니다.",
                "NO_NAME" => "아이디를 입력하세요.",
                "PYTHON_NOT_FOUND" => "Python 실행 파일을 찾을 수 없습니다.",
                "SCRIPT_NOT_FOUND" => "real_check_Face.py 파일을 찾을 수 없습니다.",
                "INVALID_JSON" => "JSON 형식의 출력이 없습니다.",
                _ => $"알 수 없는 상태({status})"
            };

            MessageBoxImage icon =
                code is "NO_FACE" or "NO_NAME" ? MessageBoxImage.Information :
                code == "INVALID_JSON" ? MessageBoxImage.Warning :
                MessageBoxImage.Error;

            MessageBox.Show(msg, "로그인", MessageBoxButton.OK, icon);
        }


        private void ShowFacePopup(string imageFileName, string text)
        {
            try
            {
                PopupImage.Source = new BitmapImage(new Uri("/Assets/" + imageFileName, UriKind.Relative));
                PopupText.Text = text;
                FacePopup.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                FacePopup.Visibility = Visibility.Collapsed;
                MessageBox.Show("팝업 표시 오류: " + ex.Message, "로그인", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine("ShowFacePopup 오류: " + ex);
            }
        }
    }
}

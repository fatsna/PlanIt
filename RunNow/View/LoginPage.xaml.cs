using Newtonsoft.Json.Linq;
using PlanIt.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PlanIt.Views
{
    public partial class LoginPage : Page
    {
        private LoginViewModel vm;

        public LoginPage()
        {
            InitializeComponent();
            vm = new LoginViewModel();
            DataContext = vm;

            PasswordBox.PasswordChanged += (s, e) =>
            {
                vm.Password = PasswordBox.Password;
            };

            // 얼굴 인식 완료 시 팝업 표시
            vm.FaceDetected += async () =>
            {
                Dispatcher.Invoke(() =>
                {
                    ShowFacePopup("check.png", "인식 완료!");
                });

                await Task.Delay(2000);
                Dispatcher.Invoke(() => FacePopup.Visibility = Visibility.Collapsed);
            };
        }

        // 회원가입 페이지로 이동
        private void Register_Click(object sender, MouseButtonEventArgs e)
        {
            var registerPage = new RegisterPage();
            NavigationService?.Navigate(registerPage);
        }

        private void FindPassword_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("비밀번호 찾기 클릭됨");
        }

        // 얼굴 인식 버튼 클릭 시
        private async void FaceRecognition_Click(object sender, RoutedEventArgs e)
        {
            ShowFacePopup("face_id.png", "얼굴 인식 중...");

            string result = await RunFaceRecognitionAsync();

            if (result.StartsWith("SUCCESS"))
            {
                try
                {
                    MessageBox.Show("얼굴 인식 성공!");
                    ShowFacePopup("check.png", $"환영합니다");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("성공 처리 중 오류: " + ex.Message);
                    Debug.WriteLine("SUCCESS 분기 예외: " + ex);
                }
            }
            else if (result == "FAIL")
            {
                ShowFacePopup("fail.png", "로그인 실패");
            }
            else if (result == "NO_FACE")
            {
                ShowFacePopup("error.png", "등록된 얼굴 없음");
            }
            else if (result == "CAMERA_ERROR")
            {
                ShowFacePopup("error.png", "카메라 오류");
            }
            else if (result == "NO_NAME")
            {
                ShowFacePopup("error.png", "아이디를 입력하세요");
            }
            else
            {
                ShowFacePopup("error.png", "알 수 없는 오류");
            }

            await Task.Delay(2000);
            FacePopup.Visibility = Visibility.Collapsed;
        }

        // 얼굴 인식 실행 (Python 프로세스 실행)
        private async Task<string> RunFaceRecognitionAsync()
        {
            try
            {
                string pythonPath = @"C:\Users\YESOM\PycharmProjects\PythonProject1\.venv\Scripts\python.exe";
                string scriptPath = @"C:\Users\YESOM\PycharmProjects\PythonProject1\real_check_Face.py";

                if (!File.Exists(pythonPath))
                {
                    MessageBox.Show("Python 실행 파일을 찾을 수 없습니다.");
                    return "PYTHON_NOT_FOUND";
                }

                if (!File.Exists(scriptPath))
                {
                    MessageBox.Show("real_check_Face.py 파일을 찾을 수 없습니다.");
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

                using (var process = Process.Start(psi))
                {
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    process.WaitForExit();

                    Debug.WriteLine("🟢 Python Output:");
                    Debug.WriteLine(output);
                    Debug.WriteLine("🔴 Python Error:");
                    Debug.WriteLine(error);

                    // JSON 라인 추출
                    string jsonLine = null;
                    foreach (var line in output.Split('\n'))
                    {
                        if (line.TrimStart().StartsWith("{") && line.TrimEnd().EndsWith("}"))
                        {
                            jsonLine = line.Trim();
                            break;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(jsonLine))
                    {
                        MessageBox.Show("JSON 형식의 출력이 없습니다.");
                        return "INVALID_JSON";
                    }

                    JObject result = JObject.Parse(jsonLine);
                    string status = result["status"]?.ToString();

                    if (status == "SUCCESS")
                    {
                        JArray embeddingArray = (JArray)result["embedding"];
                        float[] embedding = embeddingArray.ToObject<float[]>();

                        vm.FaceEmbedding = embedding;
                        Debug.WriteLine("✅ 임베딩 길이: " + embedding.Length);

                        vm.FaceDetected?.Invoke();
                        return "SUCCESS";
                    }
                    else if (status == "NO_FACE")
                    {
                        MessageBox.Show("얼굴이 인식되지 않았습니다.");
                        return "NO_FACE";
                    }
                    else if (status == "FAIL")
                    {
                        MessageBox.Show("얼굴 인식 실패");
                        return "FAIL";
                    }
                    else
                    {
                        MessageBox.Show("알 수 없는 상태 반환: " + status);
                        return "UNKNOWN";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Python 실행 오류: " + ex.Message);
                Debug.WriteLine("❌ 예외 발생: " + ex);
                return "ERROR";
            }
        }


        // 팝업 표시 메서드
        private void ShowFacePopup(string imagePath, string text)
        {
            try
            {
                PopupImage.Source = new BitmapImage(new Uri("/" + imagePath, UriKind.Relative));
                PopupText.Text = text;
                FacePopup.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show("팝업 표시 오류: " + ex.Message);
                Debug.WriteLine("ShowFacePopup 오류: " + ex);
            }
        }
    }
}

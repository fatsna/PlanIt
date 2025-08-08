using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using RunNow.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
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

            // PasswordBox 값 바인딩 수동 처리
            PasswordBox.PasswordChanged += (s, e) =>
            {
                vm.Password = PasswordBox.Password;
            };

            // 얼굴 인식 완료 이벤트
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

            string result = await RunFaceRecognitionAsync();

            switch (result)
            {
                case "SUCCESS":
                    ShowFacePopup("check.png", "환영합니다");
                    break;
                case "FAIL":
                    ShowFacePopup("fail.png", "로그인 실패");
                    break;
                case "NO_FACE":
                    ShowFacePopup("error.png", "등록된 얼굴 없음");
                    break;
                case "CAMERA_ERROR":
                    ShowFacePopup("error.png", "카메라 오류");
                    break;
                case "NO_NAME":
                    ShowFacePopup("error.png", "아이디를 입력하세요");
                    break;
                default:
                    ShowFacePopup("error.png", "알 수 없는 오류");
                    break;
            }

            await Task.Delay(2000);
            FacePopup.Visibility = Visibility.Collapsed;
        }

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

                using var process = Process.Start(psi);
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                Debug.WriteLine("?? Python Output:\n" + output);
                Debug.WriteLine("?? Python Error:\n" + error);

                // JSON 파싱
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
                    Debug.WriteLine("? 임베딩 길이: " + embedding.Length);

                    if (vm.FaceDetected != null)
                        await vm.FaceDetected.Invoke();

                    return "SUCCESS";
                }

                return status ?? "UNKNOWN";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Python 실행 오류: " + ex.Message);
                Debug.WriteLine("? 예외 발생: " + ex);
                return "ERROR";
            }
        }

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

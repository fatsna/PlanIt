using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using RunNow.Core;
using RunNow.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RunNow.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly NavigationStore _navigationStore;
        private readonly IServiceProvider _serviceProvider;
        private readonly ShareDataService _shareDataService;

        private TcpListener _listener;
        private static bool serverStarted = false;

        public Func<Task> FaceDetected;

        public LoginViewModel(
            IAuthService authService,
            NavigationStore navigationStore,
            IServiceProvider serviceProvider,
            ShareDataService shareDataService)
        {
            _authService = authService;
            _navigationStore = navigationStore;
            _serviceProvider = serviceProvider;
            _shareDataService = shareDataService;

            if (!serverStarted)
            {
                StartSocketServer();
                serverStarted = true;
            }

            FacePopupVisibility = "Collapsed";
        }

        // -------------------------
        // 입력값 + 상태
        // -------------------------
        [ObservableProperty]
        private string username;

        partial void OnUsernameChanged(string value)
        {
            // CanLogin 즉시 반영 (XAML DataTrigger) + 버튼 활성/비활성 갱신
            OnPropertyChanged(nameof(CanLogin));
            LoginCommand?.NotifyCanExecuteChanged();
        }

        [ObservableProperty]
        private string password;

        partial void OnPasswordChanged(string value)
        {
            OnPropertyChanged(nameof(CanLogin));
            LoginCommand?.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// XAML 바인딩용 - 아이디/비번 모두 입력 시 true
        /// </summary>
        public bool CanLogin =>
            !string.IsNullOrWhiteSpace(Username) &&
            !string.IsNullOrWhiteSpace(Password);

        /// <summary>
        /// 커맨드 CanExecute용 메서드 (속성과 이름 충돌 피하기)
        /// </summary>
        private bool CanExecuteLogin() => CanLogin;

        // -------------------------
        // 얼굴 임베딩
        // -------------------------
        private float[] faceEmbedding;
        public float[] FaceEmbedding
        {
            get => faceEmbedding;
            set
            {
                SetProperty(ref faceEmbedding, value);
                _ = FaceDetected?.Invoke();
                _ = HandleFaceLoginAsync();
            }
        }

        // -------------------------
        // 팝업 바인딩
        // -------------------------
        [ObservableProperty] private string facePopupText;
        [ObservableProperty] private string facePopupImagePath;
        [ObservableProperty] private string facePopupVisibility;

        // -------------------------
        // 로그인 커맨드
        // -------------------------
        [RelayCommand(CanExecute = nameof(CanExecuteLogin))]
        private async Task LoginAsync()
        {

            if (!CanLogin) return;


            JObject response = await _authService.LoginAsync(Username, Password);
            var protocol = response["protocol"]?.ToString();

            if (protocol == "1_1") // 로그인 성공
            {

                _shareDataService.User_id = Username;
                _shareDataService.Password = Password;

                await ShowPopup("로그인 성공", "success.png");

                _navigationStore.CurrentViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            }
            else
            {
                MessageBox.Show("아이디 또는 비밀번호를 잘못입력하셨습니다.");
                await ShowPopup("로그인 실패", "error.png");
            }
        }

        // -------------------------
        // 얼굴 로그인 처리
        // -------------------------
        private async Task HandleFaceLoginAsync()
        {
            Console.WriteLine("📡 HandleFaceLoginAsync 호출됨");

            var response = await _authService.FaceLoginAsync(FaceEmbedding);
            Console.WriteLine($"📨 서버 응답: {response}");

            string status = response["protocol"]?.ToString();

            if (status == "2_1")
            {
                await ShowPopup("얼굴 로그인 성공", "success.png");
                await Task.Delay(1000);
                _navigationStore.CurrentViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            }
            else
            {
                string message = response["message"]?.ToString() ?? "인증 실패";
                Console.WriteLine("⚠️ 얼굴 인증 실패: " + message);
                await ShowPopup("얼굴 인증 실패", "fail.png");
            }
        }

        // -------------------------
        // 기타 명령
        // -------------------------
        [RelayCommand]
        private async Task FaceRecognitionAsync()
        {
            await ShowPopup("얼굴 인식 중...", "face_id.png");

            var result = await RunFaceRecognitionAsync();

            string popupMessage = result switch
            {
                "SUCCESS" => "환영합니다",
                "FAIL" => "로그인 실패",
                "NO_FACE" => "등록된 얼굴 없음",
                "CAMERA_ERROR" => "카메라 오류",
                "NO_NAME" => "아이디를 입력하세요",
                _ => "알 수 없는 오류"
            };

            string image = result switch
            {
                "SUCCESS" => "check.png",
                "FAIL" => "fail.png",
                "NO_FACE" or "CAMERA_ERROR" or "NO_NAME" => "error.png",
                _ => "error.png"
            };

            await ShowPopup(popupMessage, image);
        }

        [RelayCommand]
        private void NavigateRegister()
        {
            _navigationStore.CurrentViewModel = App.Services.GetRequiredService<RegisterViewModel>();
        }

        [RelayCommand]
        private void FindPassword()
        {
            string inputId = Microsoft.VisualBasic.Interaction.InputBox(
                "비밀번호를 찾을 아이디를 입력하세요.",
                "비밀번호 찾기");

            if (string.IsNullOrWhiteSpace(inputId))
            {
                MessageBox.Show("아이디를 입력하지 않았습니다.");
                return;
            }

            MessageBox.Show($"입력한 아이디: {inputId}\n등록된 휴대폰 번호로 비밀번호 재설정 안내 문자를 보냈습니다.", "알림");
        }

        [RelayCommand]
        private async Task ResumeManage()
        {
            MessageBox.Show("ResumeManage clicked");
            var vm = _serviceProvider.GetRequiredService<ResumeManageViewModel>();
            _navigationStore.CurrentViewModel = vm;
            await Task.Yield();
            await vm.LoadResumeFromServerAsync();
        }

        // -------------------------
        // 공용 팝업
        // -------------------------
        private async Task ShowPopup(string text, string imagePath)
        {
            FacePopupText = text;
            FacePopupImagePath = "/Assets/" + imagePath;
            FacePopupVisibility = "Visible";
            await Task.Delay(2000);
            FacePopupVisibility = "Collapsed";
        }

        // -------------------------
        // 파이썬 호출 (필요 시)
        // -------------------------
        private async Task<string> RunFaceRecognitionAsync()
        {
            try
            {
                string pythonPath = @"C:\Users\YESOM\PycharmProjects\PythonProject1\.venv\Scripts\python.exe";
                string scriptPath = @"C:\Users\YESOM\PycharmProjects\PythonProject1\real_check_Face.py";

                if (!File.Exists(pythonPath) || !File.Exists(scriptPath))
                    return "SCRIPT_NOT_FOUND";

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
                await process.WaitForExitAsync();

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
                    return "INVALID_JSON";

                JObject result = JObject.Parse(jsonLine);
                string status = result["status"]?.ToString();

                if (status == "101_1")
                {
                    var embeddingArray = (JArray)result["embedding"];
                    FaceEmbedding = embeddingArray.ToObject<float[]>();
                }

                return status ?? "UNKNOWN";
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Python 오류: " + ex.Message);
                return "ERROR";
            }
        }

        // -------------------------
        // TCP 수신 (파이썬 → C#)
        // -------------------------
        private void StartSocketServer()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 5001);
                _listener.Start();

                Thread serverThread = new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            using TcpClient client = _listener.AcceptTcpClient();
                            using NetworkStream stream = client.GetStream();
                            using StreamReader reader = new StreamReader(stream, Encoding.UTF8);

                            string message = reader.ReadToEnd().Trim();
                            Console.WriteLine("[소켓 수신] " + message);

                            if (!string.IsNullOrWhiteSpace(message) && message.StartsWith("{"))
                            {
                                var result = JObject.Parse(message);

                                var status = result["status"]?.ToString();
                                if (status == "SUCCESS")
                                {
                                    var embeddingArray = (JArray)result["embedding"];
                                    if (embeddingArray != null)
                                    {
                                        FaceEmbedding = embeddingArray.ToObject<float[]>();
                                        Console.WriteLine("✅ FaceEmbedding 설정 완료");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"⚠️ 얼굴 인식 실패 또는 상태 미확인: {status}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("소켓 예외: " + ex.Message);
                        }
                    }
                });

                serverThread.IsBackground = true;
                serverThread.Start();
            }
            catch (SocketException ex)
            {
                Debug.WriteLine("소켓 오류: " + ex.Message);
            }
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using RunNow.Services;
using RunNow.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;



namespace RunNow.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly NavigationStore _navigationStore;
        private readonly IAuthService _authService;
        private readonly TcpClientService _tcpClientService;
        private readonly IServiceProvider _serviceProvider; // ✅ 추가됨

        // ✅ 생성자 수정됨
        public RegisterViewModel(NavigationStore navigationStore, IAuthService authService, TcpClientService tcpClientService, IServiceProvider serviceProvider)
        {
            _tcpClientService = tcpClientService;
            _navigationStore = navigationStore;
            _authService = authService;
            _serviceProvider = serviceProvider; // ✅ 추가됨

            Years = Enumerable.Range(1950, DateTime.Now.Year - 1949).Reverse().ToList();
            Months = Enumerable.Range(1, 12).ToList();
            Days = Enumerable.Range(1, 31).ToList();
        }

        // ==============================
        // 📌 사용자 정보
        // ==============================

        [ObservableProperty] private string name;
        [ObservableProperty] private string userId;
        [ObservableProperty] private string address;

        [ObservableProperty] private string phonePart1;
        [ObservableProperty] private string phonePart2;
        [ObservableProperty] private string phonePart3;

        public string PhoneNumber => $"{PhonePart1}{PhonePart2}{PhonePart3}";

        // ==============================
        // 📌 성별
        // ==============================

        [ObservableProperty] private bool isMale;
        [ObservableProperty] private bool isFemale;

        // ==============================
        // 📌 생년월일
        // ==============================

        public List<int> Years { get; }
        public List<int> Months { get; }
        public List<int> Days { get; }


        [ObservableProperty] private int selectedYear;
        [ObservableProperty] private int selectedMonth;
        [ObservableProperty] private int selectedDay;

        public DateTime BirthDate => new DateTime(SelectedYear, SelectedMonth, SelectedDay);

        // ==============================
        // 📌 비밀번호 관련
        // ==============================


        private string password;
        private string confirmPassword;


        [ObservableProperty] private bool isPasswordMismatch;


        public void SetPassword(string pwd)
        {
            password = pwd;
            ValidatePasswordMatch();
            ValidateForm();

        }

        public void SetConfirmPassword(string pwd)
        {
            confirmPassword = pwd;
            ValidatePasswordMatch();
            ValidateForm();
        }

        private void ValidatePasswordMatch()
        {
            IsPasswordMismatch = password != confirmPassword;
        }

        // ==============================
        // 📌 얼굴 임베딩
        // ==============================

        [ObservableProperty] private float[] faceEmbedding;

        // ==============================
        // 📌 중복 체크 및 유효성
        // ==============================

        [ObservableProperty] private bool isUserIdChecked;
        [ObservableProperty] private bool canSubmit;

        [RelayCommand]
        private async Task CheckDuplicateAsync()
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                MessageBox.Show("아이디를 입력하세요.");
                return;
            }
            try
            {
                var response = await _authService.CheckDuplicateIdAsync(UserId);
                string status = response["protocol"]?.ToString();

                if (status == "3_1")
                {
                    MessageBox.Show("✅ 사용 가능한 아이디입니다.");
                    IsUserIdChecked = true;
                }
                else if (status == "3_2")
                {
                    MessageBox.Show("❌ 이미 사용 중인 아이디입니다.");
                    IsUserIdChecked = false;
                }
                else
                {
                    MessageBox.Show($"⚠️ 알 수 없는 응답: {status}");
                    IsUserIdChecked = false;
                }

                ValidateForm();  // 버튼 활성화 여부 갱신
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ 중복 검사 중 오류 발생: " + ex.Message);
            }
        }

        private void ValidateForm()
        {
            CanSubmit = true;
            //CanSubmit =
            //    !string.IsNullOrWhiteSpace(Name) &&
            //    !string.IsNullOrWhiteSpace(UserId) &&
            //    !string.IsNullOrWhiteSpace(password) &&
            //    password == confirmPassword &&
            //    !string.IsNullOrWhiteSpace(Address) &&
            //    !string.IsNullOrWhiteSpace(PhonePart1) &&
            //    !string.IsNullOrWhiteSpace(PhonePart2) &&
            //    !string.IsNullOrWhiteSpace(PhonePart3) &&
            //    (IsMale || IsFemale) &&
            //    SelectedYear > 0 &&
            //    SelectedMonth > 0 &&
            //    SelectedDay > 0 &&
            //    IsUserIdChecked;
        }


        // ==============================
        // 📌 얼굴 등록
        // ==============================

        [RelayCommand]
        private async Task FaceRegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                MessageBox.Show("아이디를 먼저 입력하세요.");
                return;
            }

            string pythonPath = @"C:\Users\YESOM\PycharmProjects\PythonProject1\.venv\Scripts\python.exe";
            string scriptPath = @"C:\Users\YESOM\PycharmProjects\PythonProject1\real_regiface.py";

            if (!File.Exists(pythonPath))
            {
                MessageBox.Show("Python 경로가 올바르지 않습니다.");
                return;
            }

            if (!File.Exists(scriptPath))
            {
                MessageBox.Show("스크립트 파일이 존재하지 않습니다.");
                return;
            }

            var psi = new ProcessStartInfo
            {
                FileName = pythonPath,
                Arguments = $"\"{scriptPath}\" \"{UserId}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            try
            {
                using var process = Process.Start(psi);
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                Debug.WriteLine("Python STDOUT: " + output);
                Debug.WriteLine("Python STDERR: " + error);

                var jsonLine = output.Split('\n').LastOrDefault(l => l.Trim().StartsWith("{"));

                if (string.IsNullOrWhiteSpace(jsonLine))
                {
                    MessageBox.Show("❌ 얼굴 인식 결과가 비어 있거나 JSON 형식이 아닙니다.");
                    return;
                }

                var result = JObject.Parse(jsonLine);

                string status = result["status"]?.ToString();

                switch (status)
                {
                    case "SUCCESS":
                        faceEmbedding = result["embedding"].ToObject<float[]>();
                        MessageBox.Show("😀 얼굴 등록이 완료되었습니다!");
                        break;
                    case "NO_FACE":
                        MessageBox.Show("😢 얼굴이 감지되지 않았습니다.");
                        break;
                    case "CAMERA_ERROR":
                        MessageBox.Show("📷 카메라 오류 발생.");
                        break;
                    default:
                        MessageBox.Show($"⚠️ 알 수 없는 상태: {status}");
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Python 실행 오류: " + ex.Message);
            }
        }

        // ==============================
        // 📌 가입 완료
        // ==============================

        [RelayCommand]
        private async Task Submit()
        {
            MessageBox.Show("✅ SubmitCommand 눌림");

            // 프로토콜 고정: 4_0
            var cppPayload = new JObject
            {
                ["protocol"] = "4_0",
                ["name"] = Name,
                ["userid"] = UserId,
                ["pw"] = password,
                ["birth"] = BirthDate.ToString("yyyy-MM-dd"),
                ["gender"] = IsMale ? 0 : 1,
                ["address"] = Address,
                ["phone"] = PhoneNumber,
                ["faceembedding"] = FaceEmbedding != null ? new JArray(FaceEmbedding) : null
            };
            try
            {
                MessageBox.Show("📡 메인 서버에 회원가입 정보 전송 중...");
                var response = await _authService.RegisterAsync(cppPayload);  // 동일한 payload 사용

                string status = response["protocol"]?.ToString();

                if (status == "4_1")
                {
                    MessageBox.Show("🎉 회원가입이 완료되었습니다!");
                    _navigationStore.CurrentViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
                    return;
                }
                else
                {
                    string reason = response["reason"]?.ToString();
                    MessageBox.Show($"❌ 회원가입 실패: {reason ?? status ?? "알 수 없음"}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ 회원가입 중 예외 발생: " + ex.Message);
            }
        }


        [RelayCommand]
        private void Back()
        {
            //  수정됨: LoginViewModel을 new로 생성하지 말고, 서비스에서 가져오기
            _navigationStore.CurrentViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
        }
    }
}

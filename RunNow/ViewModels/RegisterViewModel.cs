using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using RunNow.Core;
using RunNow.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RunNow.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly NavigationStore _navigationStore;
        private readonly IAuthService _authService;
        private readonly TcpClientService _tcpClientService;
        private readonly IServiceProvider _serviceProvider;

        // ==============================
        // 📌 생성자
        // ==============================
        public RegisterViewModel(
            NavigationStore navigationStore,
            IAuthService authService,
            TcpClientService tcpClientService,
            IServiceProvider serviceProvider)
        {
            _tcpClientService = tcpClientService;
            _navigationStore = navigationStore;
            _authService = authService;
            _serviceProvider = serviceProvider;

            Years = Enumerable.Range(1950, DateTime.Now.Year - 1949).Reverse().ToList();
            Months = Enumerable.Range(1, 12).ToList();
            Days = Enumerable.Range(1, 31).ToList();
        }

        // ==============================
        // 📌 내부 상태 / 유틸
        // ==============================
        private enum DupIdStatus { Available, Taken, Unknown }

        private static DupIdStatus ParseDupStatus(JObject resp)
        {
            var code = resp?["protocol"]?.ToString();
            return code switch
            {
                "3_1" => DupIdStatus.Available, // 사용 가능
                "3_2" => DupIdStatus.Taken,     // 이미 사용 중
                _ => DupIdStatus.Unknown
            };
        }

        // 비밀번호는 PasswordBox 연동을 위해 필드로만 관리
        private string password;
        private string confirmPassword;

        // 전화번호 정규화 재진입 방지 플래그
        private bool _suppressPhoneSanitize;

        private static string DigitsOnly(string s, int maxLen) =>
            string.IsNullOrEmpty(s) ? string.Empty : new string(s.Where(char.IsDigit).Take(maxLen).ToArray());

        // ==============================
        // 📌 사용자 기본 정보
        // ==============================
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private string name;

        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private string userId;

        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private string address;

        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private string phonePart1;

        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private string phonePart2;

        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private string phonePart3;

        public string PhoneNumber => $"{PhonePart1}{PhonePart2}{PhonePart3}";

        // 아이디가 바뀌면 기존 중복확인 결과는 무효
        partial void OnUserIdChanged(string value)
        {
            IsUserIdChecked = false;
            SubmitCommand?.NotifyCanExecuteChanged();
        }

        // 전화번호: 숫자만 유지 + 길이 강제 (3/4/4)
        partial void OnPhonePart1Changed(string value)
        {
            if (_suppressPhoneSanitize) return;
            var sanitized = DigitsOnly(value, 3);
            if (sanitized != value)
            {
                _suppressPhoneSanitize = true;
                PhonePart1 = sanitized;
                _suppressPhoneSanitize = false;
            }
            SubmitCommand?.NotifyCanExecuteChanged();
        }

        partial void OnPhonePart2Changed(string value)
        {
            if (_suppressPhoneSanitize) return;
            var sanitized = DigitsOnly(value, 4);
            if (sanitized != value)
            {
                _suppressPhoneSanitize = true;
                PhonePart2 = sanitized;
                _suppressPhoneSanitize = false;
            }
            SubmitCommand?.NotifyCanExecuteChanged();
        }

        partial void OnPhonePart3Changed(string value)
        {
            if (_suppressPhoneSanitize) return;
            var sanitized = DigitsOnly(value, 4);
            if (sanitized != value)
            {
                _suppressPhoneSanitize = true;
                PhonePart3 = sanitized;
                _suppressPhoneSanitize = false;
            }
            SubmitCommand?.NotifyCanExecuteChanged();
        }

        // ==============================
        // 📌 성별
        // ==============================
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private bool isMale;

        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private bool isFemale;

        // ==============================
        // 📌 생년월일
        // ==============================
        public List<int> Years { get; }
        public List<int> Months { get; }
        public List<int> Days { get; }

        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private int selectedYear;

        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private int selectedMonth;

        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private int selectedDay;

        public DateTime BirthDate => new DateTime(SelectedYear, SelectedMonth, SelectedDay);

        // ==============================
        // 📌 비밀번호 관련
        // ==============================
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private bool isPasswordMismatch;

        public void SetPassword(string pwd)
        {
            password = pwd;
            ValidatePasswordMatch();
            SubmitCommand?.NotifyCanExecuteChanged();
        }

        public void SetConfirmPassword(string pwd)
        {
            confirmPassword = pwd;
            ValidatePasswordMatch();
            SubmitCommand?.NotifyCanExecuteChanged();
        }

        private void ValidatePasswordMatch()
        {
            IsPasswordMismatch = password != confirmPassword;
        }

        // ==============================
        // 📌 얼굴 임베딩
        // ==============================
        [ObservableProperty]
        private float[] faceEmbedding;

        // ==============================
        // 📌 중복 체크 및 유효성
        // ==============================
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private bool isUserIdChecked;

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
                switch (ParseDupStatus(response))
                {
                    case DupIdStatus.Available:
                        IsUserIdChecked = true;
                        MessageBox.Show("✅ 사용 가능한 아이디입니다.");
                        break;

                    case DupIdStatus.Taken:
                        IsUserIdChecked = false;
                        MessageBox.Show("❌ 이미 사용 중인 아이디입니다.");
                        break;

                    default:
                        IsUserIdChecked = false;
                        MessageBox.Show($"⚠️ 알 수 없는 응답: {response?["protocol"]?.ToString() ?? "null"}");
                        break;
                }
            }
            catch (Exception ex)
            {
                IsUserIdChecked = false;
                MessageBox.Show("❌ 중복 검사 중 오류 발생: " + ex.Message);
            }
            finally
            {
                SubmitCommand?.NotifyCanExecuteChanged();
            }
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
                        FaceEmbedding = result["embedding"].ToObject<float[]>();
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
        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task Submit()
        {
            MessageBox.Show("✅ SubmitCommand 눌림");

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
                var response = await _authService.RegisterAsync(cppPayload);

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

        private bool CanSubmit()
        {
            return
                !string.IsNullOrWhiteSpace(Name) &&
                !string.IsNullOrWhiteSpace(UserId) &&
                !string.IsNullOrWhiteSpace(password) &&
                password == confirmPassword &&
                !string.IsNullOrWhiteSpace(Address) &&
                // 전화번호: 반드시 3-4-4가 꽉 차야 함
                PhonePart1?.Length == 3 &&
                PhonePart2?.Length == 4 &&
                PhonePart3?.Length == 4 &&
                (IsMale || IsFemale) &&
                SelectedYear > 0 &&
                SelectedMonth > 0 &&
                SelectedDay > 0 &&
                IsUserIdChecked;
        }

        // ==============================
        // 📌 뒤로가기
        // ==============================
        [RelayCommand]
        private void Back()
        {
            _navigationStore.CurrentViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
        }
    }
}

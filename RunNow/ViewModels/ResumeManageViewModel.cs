using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using RunNow.Core;
using RunNow.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace RunNow.ViewModels
{
    public partial class ResumeManageViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly TcpClientService _tcpClientService;
        private readonly IAuthService _authService;
        private readonly ShareDataService _share;

        [ObservableProperty] private string name;
        [ObservableProperty] private string emailId;
        [ObservableProperty] private string selectedEmailDomain;
        [ObservableProperty] private string selectedYear;
        [ObservableProperty] private string selectedMonth;
        [ObservableProperty] private string selectedDay;
        [ObservableProperty] private string selectedPhonePrefix;
        [ObservableProperty] private string phoneMid;
        [ObservableProperty] private string phoneEnd;
        [ObservableProperty] private string address;
        [ObservableProperty] private string career;
        [ObservableProperty] private string notes;
        [ObservableProperty] private string selectedCertificate;

        public ObservableCollection<string> Years { get; } = new();
        public ObservableCollection<string> Months { get; } = new();
        public ObservableCollection<string> Days { get; } = new();
        public ObservableCollection<string> EmailDomains { get; } = new()
        {
            "naver.com", "hanmail.net", "gmail.com", "nate.com", "kakao.com", "icloud.com", "outlook.com"
        };
        public ObservableCollection<string> PhonePrefixes { get; } = new()
        {
            "010", "011", "016", "017", "018", "019"
        };
        public ObservableCollection<string> Certificates { get; } = new();

        public ResumeManageViewModel(
            IDialogService dialogService,
            TcpClientService tcpClientService,
            IAuthService authService,
            ShareDataService shareDataService)
        {
            _dialogService = dialogService;
            _tcpClientService = tcpClientService;
            _authService = authService;
            _share = shareDataService;

            for (int y = 1950; y <= 2010; y++) Years.Add(y.ToString());
            for (int m = 1; m <= 12; m++) Months.Add(m.ToString("D2"));
            for (int d = 1; d <= 31; d++) Days.Add(d.ToString("D2"));
        }

        [RelayCommand]
        private void SaveResume()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Address))
            {
                _dialogService.ShowMessage("필수 정보를 입력하세요.", "입력 오류");
                return;
            }

            string birthdate = $"{SelectedYear}-{SelectedMonth}-{SelectedDay}";
            string email = $"{EmailId}@{SelectedEmailDomain}";
            string phone = $"{SelectedPhonePrefix}-{PhoneMid}-{PhoneEnd}";

            string summary =
                $"이름: {Name}\n생년월일: {birthdate}\n이메일: {email}\n전화: {phone}\n주소: {Address}\n\n경력사항:\n{Career}\n\n특이사항:\n{Notes}";
            _dialogService.ShowMessage(summary, "입력 내용");
        }

        [RelayCommand]
        private void OpenCertificate()
        {
            string selected = _dialogService.ShowCertificateDialog();
            if (!string.IsNullOrWhiteSpace(selected))
            {
                SelectedCertificate = selected;
                if (!Certificates.Contains(selected))
                    Certificates.Add(selected);
            }
        }

        [RelayCommand]
        private void RemoveCertificate(string cert)
        {
            if (Certificates.Contains(cert))
                Certificates.Remove(cert);
        }

        [RelayCommand]
        private async Task SendToServer()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Address))
            {
                _dialogService.ShowMessage("필수 정보를 입력하세요.", "입력 오류");
                return;
            }

            try
            {
                var resumeData = new JObject
                {
                    ["u_id"] = _share.User_id,
                    ["protocol"] = "7_0",
                    ["name"] = Name,
                    ["birth"] = $"{SelectedYear}-{SelectedMonth}-{SelectedDay}",
                    ["email"] = $"{EmailId}@{SelectedEmailDomain}",
                    ["phone"] = $"{SelectedPhonePrefix}-{PhoneMid}-{PhoneEnd}",
                    ["address"] = Address,
                    ["career"] = Career,
                    ["notes"] = Notes,
                    ["license"] = JArray.FromObject(Certificates)
                };

                _dialogService.ShowMessage("서버에 데이터를 전송하는 중...", "알림");
                var response = await _authService.SaveResumeAsync(resumeData);

                if (response != null)
                {
                    string protocol = response["protocol"]?.ToString();
                    string message = response["message"]?.ToString() ?? "서버로부터 응답 메시지를 받지 못했습니다.";

                    if (protocol == "7_1")
                    {
                        _dialogService.ShowMessage("이력서가 성공적으로 저장되었습니다.", "전송 성공");
                    }
                    else
                    {
                        _dialogService.ShowMessage($"이력서 저장 실패: {message}", "전송 실패");
                    }
                }
                else
                {
                    _dialogService.ShowMessage("서버로부터 유효한 응답을 받지 못했습니다.", "전송 실패");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"서버 통신 중 오류가 발생했습니다: {ex.Message}", "통신 오류");
            }
        }

        private static (string yy, string mm, string dd) SplitBirth(string birth)
        {
            if (DateTime.TryParse(birth, out var dt))
                return (dt.Year.ToString(), dt.Month.ToString("D2"), dt.Day.ToString("D2"));

            var digits = new string(birth.Where(char.IsDigit).ToArray());
            if (digits.Length >= 8)
                return (digits.Substring(0, 4), digits.Substring(4, 2), digits.Substring(6, 2));

            return ("", "", "");
        }

        // 011/016/017/018/019, 지역번호 등도 커버
        private static (string p1, string p2, string p3) SplitPhone(string phone)
        {
            var digits = new string(phone.Where(char.IsDigit).ToArray());

            // +82 10 xxxx xxxx → 010-xxxx-xxxx
            if (digits.StartsWith("82") && digits.Length >= 11)
            {
                var rest = digits.Substring(2);
                if (rest.Length >= 10 && rest.StartsWith("10"))
                    return ("010", rest.Substring(2, 4), rest.Substring(6, 4));
            }

            // 01X 휴대전화
            if (digits.Length == 11 && digits.StartsWith("01"))
                return (digits.Substring(0, 3), digits.Substring(3, 4), digits.Substring(7, 4));

            // 서울 02
            if ((digits.Length == 10 || digits.Length == 11) && digits.StartsWith("02"))
            {
                return ("02",
                    digits.Substring(2, digits.Length == 10 ? 4 : 5),
                    digits.Substring(digits.Length - 4, 4));
            }

            // 기타 지역번호(0xx)
            if ((digits.Length == 10 || digits.Length == 11) && digits.StartsWith("0"))
            {
                return (digits.Substring(0, 3),
                    digits.Substring(3, digits.Length - 7),
                    digits.Substring(digits.Length - 4, 4));
            }

            // 구분자 분해
            var parts = phone.Split('-', ' ', '/').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            if (parts.Length >= 3)
                return (parts[0], parts[1], parts[2]);

            return ("", "", "");
        }

        public async Task LoadResumeFromServerAsync()
        {
            try
            {
                _dialogService?.ShowMessage("이력서 조회 요청(6_0) 전송", "알림");
                Console.WriteLine("▶ 6_0 요청 전송 준비");

                var payload = new JObject
                {
                    ["protocol"] = "6_0",
                    ["u_id"] = _share?.User_id
                };

                Console.WriteLine("▶ payload: " + payload.ToString());
                var resp = await _authService.QueryResumeAsync(payload);

                if (resp == null)
                {
                    _dialogService?.ShowMessage("서버 응답 없음 (null)", "오류");
                    Console.WriteLine("⛔ resp == null");
                    return;
                }

                Console.WriteLine("◀ resp: " + resp.ToString());

                var respProto = resp["protocol"]?.ToString();
                if (respProto == "6_0")
                {
                    _dialogService?.ShowMessage("전송 실패(네트워크 오류 추정): 서버 대신 원본 payload가 돌아왔습니다.", "오류");
                    return;
                }

                if (respProto != "6_1")
                {
                    var err = resp["message"]?.ToString() ?? resp["error"]?.ToString() ?? "이력서 조회 실패";
                    _dialogService?.ShowMessage($"서버 오류/미매칭: {respProto} / {err}", "오류");
                    return;
                }

                // ✅ payload가 없으면 루트에서 바로 읽기
                var data = resp["payload"] ?? resp;
                if (data == null || !data.HasValues)
                {
                    _dialogService?.ShowMessage("이력서 데이터가 비어있습니다.", "안내");
                    return;
                }

                // 매핑
                Name = data["name"]?.ToString() ?? "";
                Address = data["address"]?.ToString() ?? "";

                var birthRaw = data["birthdate"]?.ToString() ?? data["birth"]?.ToString();
                if (!string.IsNullOrWhiteSpace(birthRaw))
                {
                    var (yy, mm, dd) = SplitBirth(birthRaw);
                    if (!string.IsNullOrEmpty(yy)) SelectedYear = yy;
                    if (!string.IsNullOrEmpty(mm)) SelectedMonth = mm;
                    if (!string.IsNullOrEmpty(dd)) SelectedDay = dd;
                }

                var phoneRaw = data["phone"]?.ToString();
                if (!string.IsNullOrWhiteSpace(phoneRaw))
                {
                    var (p1, p2, p3) = SplitPhone(phoneRaw);
                    if (!string.IsNullOrEmpty(p1)) SelectedPhonePrefix = p1;
                    if (!string.IsNullOrEmpty(p2)) PhoneMid = p2;
                    if (!string.IsNullOrEmpty(p3)) PhoneEnd = p3;
                }

                var emailRaw = data["email"]?.ToString();
                if (!string.IsNullOrWhiteSpace(emailRaw) && emailRaw.Contains("@"))
                {
                    var parts = emailRaw.Split('@');
                    EmailId = parts[0];
                    SelectedEmailDomain = parts.Length > 1 ? parts[1] : "";
                }

                var license = data["license"] as JArray;
                if (license != null)
                {
                    Certificates.Clear();
                    foreach (var x in license.Select(t => t?.ToString()).Where(s => !string.IsNullOrWhiteSpace(s)))
                        if (!Certificates.Contains(x!)) Certificates.Add(x!);
                }

                Career = data["career"]?.ToString() ?? Career;
                Notes = data["notes"]?.ToString() ?? Notes;

                _dialogService?.ShowMessage("이력서 조회 완료", "완료");
            }
            catch (Exception ex)
            {
                _dialogService?.ShowMessage($"이력서 조회 중 오류: {ex.Message}", "오류");
                Console.WriteLine("⛔ exception: " + ex);
            }
        }
    }
}

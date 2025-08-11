using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using RunNow.Core;
using RunNow.Services;
using System;          // ↑ 필요
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Printing;
using System.Windows;
using System.Windows.Shapes;
using System.Xml.Linq;

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

        public ResumeManageViewModel(IDialogService dialogService, TcpClientService tcpClientService, IAuthService authService, ShareDataService shareDataService)
        {
            _dialogService = dialogService;
            _tcpClientService = tcpClientService;
            _authService = authService;
            this._share = shareDataService;

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

            string summary = $"이름: {Name}\n생년월일: {birthdate}\n이메일: {email}\n전화: {phone}\n주소: {Address}\n\n경력사항:\n{Career}\n\n특이사항:\n{Notes}";
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

        // 서버 전송을 위한 새로운 Command 추가
        [RelayCommand]
        private async Task SendToServer() // 비동기 메서드로 변경
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Address))
            {
                _dialogService.ShowMessage("필수 정보를 입력하세요.", "입력 오류");
                return;
            }

            try
            {
                // 2. JObject로 데이터 생성
                var resumeData = new JObject
                {
                    ["u_id"] = this._share.User_id,
                    //["u_id"] = "YYS",
                    ["protocol"] = "7_0", // ✅ 프로토콜 추가
                    ["name"] = Name,
                    ["birth"] = $"{SelectedYear}-{SelectedMonth}-{SelectedDay}",
                    ["email"] = $"{EmailId}@{SelectedEmailDomain}",
                    ["phone"] = $"{SelectedPhonePrefix}-{PhoneMid}-{PhoneEnd}",
                    ["address"] = Address,
                    ["career"] = Career,
                    ["notes"] = Notes,
                    ["license"] = JArray.FromObject(Certificates) // ObservableCollection을 JArray로 변환
                };
                // 3. 서버에 데이터 전송
                _dialogService.ShowMessage("서버에 데이터를 전송하는 중...", "알림");
                var response = await _authService.SaveResumeAsync(resumeData);

                // 4. 서버 응답 처리
                if (response != null)
                {
                    string protocol = response["protocol"]?.ToString();
                    string message = response["message"]?.ToString() ?? "서버로부터 응답 메시지를 받지 못했습니다.";

                    if (protocol == "7_1") // ✅ 서버 응답 프로토콜에 따른 처리
                    {
                        _dialogService.ShowMessage("이력서가 성공적으로 저장되었습니다.", "전송 성공");
                        //하고 홈화면으로 돌아갈것인가 ?
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
            // 허용: "1990-07-15", "1990/07/15", "19900715"
            if (DateTime.TryParse(birth, out var dt))
                return (dt.Year.ToString(), dt.Month.ToString("D2"), dt.Day.ToString("D2"));

            var digits = new string(birth.Where(char.IsDigit).ToArray());
            if (digits.Length >= 8)
                return (digits.Substring(0, 4), digits.Substring(4, 2), digits.Substring(6, 2));

            return ("", "", "");
        }

        private static (string p1, string p2, string p3) SplitPhone(string phone)
        {
            // E.164(+821012345678) 또는 010-1234-5678 둘 다 처리
            var digits = new string(phone.Where(char.IsDigit).ToArray());

            // +82 10 1234 5678 → 010-1234-5678 변환
            if (digits.StartsWith("82") && digits.Length >= 11)
            {
                // 82 10 xxxx xxxx
                var rest = digits.Substring(2);
                if (rest.StartsWith("10") && rest.Length >= 10)
                    return ("010", rest.Substring(2, 4), rest.Substring(6, 4));
            }

            // 국내 포맷 가정: 010xxxxxxxx
            if (digits.Length == 11 && digits.StartsWith("010"))
                return ("010", digits.Substring(3, 4), digits.Substring(7, 4));

            // 하이픈 기반 분해 시도
            var parts = phone.Split('-', ' ', '/').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            if (parts.Length >= 3)
                return (parts[0], parts[1], parts[2]);

            return ("", "", "");
        }



        public async Task LoadResumeFromServerAsync()
        {
            try
            {
                // (A) 보내기 직전 알림/로그
                _dialogService?.ShowMessage("이력서 조회 요청(6_0) 전송", "알림");
                Console.WriteLine("▶ 6_0 요청 전송 준비");

                var payload = new JObject
                {
                    ["protocol"] = "6_0",
                    ["u_id"] = _share?.User_id   // 서버가 user_id/u_id 요구하면 바꿔보자
                                               // ["user_id"] = _share?.User_id,
                                               // ["u_id"] = _share?.User_id,
                };

                Console.WriteLine("▶ payload: " + payload.ToString());

                var resp = await _authService.QueryResumeAsync(payload);

                // (B) 응답 로그
                if (resp == null)
                {
                    _dialogService?.ShowMessage("서버 응답 없음 (null)", "오류");
                    Console.WriteLine("⛔ resp == null");
                    return;
                }

                Console.WriteLine("◀ resp: " + resp.ToString());

                // (C) TcpClientService가 예외에서 payload 그대로 돌려보내는 경우 구분
                var respProto = resp["protocol"]?.ToString();
                if (respProto == "6_0")
                {
                    _dialogService?.ShowMessage("전송 실패(네트워크 오류 추정): 서버 대신 원본 payload가 돌아왔습니다.", "오류");
                    return;
                }

                // (D) 성공 코드 확인: 실제 서버 코드로 바꿔!
                //    106_0가 맞는지 로그로 먼저 확인
                if (respProto != "106_0" /* && respProto != "6_1" 등 서버 실제값 */)
                {
                    var err = resp["message"]?.ToString() ?? resp["error"]?.ToString() ?? "이력서 조회 실패";
                    _dialogService?.ShowMessage($"서버 오류/미매칭: {respProto} / {err}", "오류");
                    return;
                }

                var data = resp["payload"];
                if (data == null)
                {
                    _dialogService?.ShowMessage("이력서 데이터가 비어있습니다.", "안내");
                    return;
                }

                // 이하 매핑 그대로…
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

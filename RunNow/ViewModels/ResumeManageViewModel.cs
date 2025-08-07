using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RunNow.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace RunNow.ViewModels
{
    public partial class ResumeManageViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;

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

        public ResumeManageViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;

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
    }
}

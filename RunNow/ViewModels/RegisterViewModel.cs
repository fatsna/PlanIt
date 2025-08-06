using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RunNow.Services;

namespace RunNow.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        [ObservableProperty] private string name;
        [ObservableProperty] private string userId;
        [ObservableProperty] private string address;
        [ObservableProperty] private string phonePart1;
        [ObservableProperty] private string phonePart2;
        [ObservableProperty] private string phonePart3;
        [ObservableProperty] private bool isPasswordMismatch;
        [ObservableProperty] private bool canSubmit;

        public List<int> Years { get; } = Enumerable.Range(1950, 80).Reverse().ToList();
        public List<int> Months { get; } = Enumerable.Range(1, 12).ToList();
        public List<int> Days { get; } = Enumerable.Range(1, 31).ToList();

        [ObservableProperty] private int selectedYear;
        [ObservableProperty] private int selectedMonth;
        [ObservableProperty] private int selectedDay;

        [ObservableProperty] private bool isMale;
        [ObservableProperty] private bool isFemale;

        private string password;
        private string confirmPassword;

        [RelayCommand]
        private void CheckDuplicate()
        {
            // 아이디 중복 체크 로직
        }

        public void SetPassword(string pwd)
        {
            password = pwd;
            ValidatePassword();
        }

        public void SetConfirmPassword(string pwd)
        {
            confirmPassword = pwd;
            ValidatePassword();
        }

        private void ValidatePassword()
        {
            IsPasswordMismatch = password != confirmPassword;
            CanSubmit = !IsPasswordMismatch && !string.IsNullOrEmpty(password);
        }

        [RelayCommand]
        private void Submit()
        {
            // 회원가입 처리 로직
        }

        [RelayCommand]
        private void FaceRegister()
        {
            // 얼굴 등록 로직
        }

        private readonly NavigationStore _navigationStore;
        private readonly IAuthService _authService;

        public RegisterViewModel(NavigationStore navigationStore)
        {
            _navigationStore = navigationStore;
        }

        [RelayCommand]
        private void Back()
        {
            _navigationStore.CurrentViewModel = new LoginViewModel(_authService, _navigationStore);
        }
        
    }
}

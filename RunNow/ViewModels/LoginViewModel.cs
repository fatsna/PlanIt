using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using RunNow.Core;
using RunNow.Services;
using System.Threading.Tasks;

namespace RunNow.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly NavigationStore _navigationStore;

        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isFacePopupVisible;

        [ObservableProperty]
        private string facePopupText;

        [ObservableProperty]
        private string facePopupImagePath;

        public LoginViewModel(IAuthService authService, NavigationStore navigationStore)
        {
            _authService = authService;
            _navigationStore = navigationStore;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ShowPopup("아이디/비번 입력 필요", "warning.png");
                return;
            }

            var result = await _authService.LoginAsync(Username, Password);

            if (result)
            {
                ShowPopup("로그인 성공", "success.png");
                await Task.Delay(1000);

                // ✅ 화면 전환
                _navigationStore.CurrentViewModel = App.Services.GetRequiredService<MainViewModel>();
            }
            else
            {
                ShowPopup("로그인 실패", "error.png");
            }
        }

        [RelayCommand]
        private async Task FaceLoginAsync()
        {
            ShowPopup("얼굴 인식 중...", "face_scan.png");

            var result = await _authService.FaceLoginAsync();

            if (result)
            {
                ShowPopup("인식 성공", "success.png");
                await Task.Delay(1000);

                // ✅ 로그인 성공 → 메인 화면 이동
                _navigationStore.CurrentViewModel = App.Services.GetRequiredService<MainViewModel>();
            }
            else
            {
                ShowPopup("인식 실패", "error.png");
            }
        }

        [RelayCommand]
        private void NavigateRegister()
        {
            // TODO: 회원가입 화면으로 이동
             _navigationStore.CurrentViewModel = App.Services.GetRequiredService<RegisterViewModel>();
        }

        [RelayCommand]
        private void FindPassword()
        {
            // TODO: 비밀번호 찾기 화면으로 이동
        }

        private async void ShowPopup(string message, string imagePath)
        {
            FacePopupText = message;
            FacePopupImagePath = imagePath;
            IsFacePopupVisible = true;
            await Task.Delay(2000);
            IsFacePopupVisible = false;
        }
    }
}

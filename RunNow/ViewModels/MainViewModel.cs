using CommunityToolkit.Mvvm.ComponentModel;

namespace RunNow.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        // 홈 화면 전용 데이터 및 상태 관리
        [ObservableProperty]
        private string welcomeMessage = "홈 화면에 오신 것을 환영합니다!";
    }
}

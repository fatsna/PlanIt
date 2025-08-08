using Microsoft.Extensions.DependencyInjection;
using RunNow.Core;
using RunNow.ViewModels;
using System.Windows;
using RunNow.Views;

namespace RunNow
{
    public partial class App : Application
    {
        public static ServiceProvider Services { get; private set; }

        public App()
        {
            InitializeComponent();
            Services = DependencyInjection.ConfigureServices();  // DI 설정
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1) 초기 ViewModel 세팅
            var store = Services.GetRequiredService<NavigationStore>();

            store.CurrentViewModel = Services.GetRequiredService<LoginViewModel>();  // ✅ 수정: 처음엔 로그인 화면으로 시작
            //store.CurrentViewModel = Services.GetRequiredService<MainViewModel>();  // 개발 중 메인 뷰 테스트할 때만 주석 해제

            // 2) DI에서 팩토리(위에서 등록한 람다)로 MainWindow 생성
            var mainWindow = Services.GetRequiredService<MainWindow>();

            mainWindow.Show();
        }
    }
}

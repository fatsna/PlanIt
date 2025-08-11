using Microsoft.Extensions.DependencyInjection;
using RunNow.Core;
using RunNow.ViewModels;
using System.Windows;
using RunNow.Views;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;


namespace RunNow
{
    public partial class App : Application
    {
        public static ServiceProvider Services { get; private set; }

        public App()
        {
            InitializeComponent();
            Services = DependencyInjection.ConfigureServices();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // LiveCharts 전역 한글 폰트 지정 (한 번만 호출)
            // 시스템에 설치된 CJK 폰트 중 한글이 포함된 폰트를 자동 매칭해 사용 (배포 환경 안전)
            LiveCharts.Configure(cfg =>
                cfg.AddSkiaSharp()
                   .HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('헬')));

            // 1) 초기 ViewModel 세팅
            var store = Services.GetRequiredService<NavigationStore>();
            //store.CurrentViewModel = Services.GetRequiredService<MainViewModel>();
            store.CurrentViewModel = Services.GetRequiredService<LoginViewModel>();

            // DI에서 팩토리(위에서 등록한 람다)로 MainWindow 생성
            var mainWindow = Services.GetRequiredService<MainWindow>();

            mainWindow.Show();
        }
    }
}

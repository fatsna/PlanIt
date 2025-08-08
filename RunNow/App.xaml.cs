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

//<<<<<<< HEAD
            // 1) 초기 ViewModel 세팅
            var store = Services.GetRequiredService<NavigationStore>();

//=======
//            // ✅ LiveCharts 전역 한글 폰트 지정 (한 번만 호출)
//            LiveCharts.Configure(cfg =>
//                cfg.AddSkiaSharp()
//                   .HasGlobalSKTypeface(SKTypeface.FromFamilyName("Malgun Gothic")));

//            // 1) 초기 ViewModel 세팅
//            var store = Services.GetRequiredService<NavigationStore>();
//>>>>>>> HJY
            store.CurrentViewModel = Services.GetRequiredService<MainViewModel>();
            //store.CurrentViewModel = Services.GetRequiredService<LoginViewModel>();

            // DI에서 팩토리(위에서 등록한 람다)로 MainWindow 생성
            var mainWindow = Services.GetRequiredService<MainWindow>();

            mainWindow.Show();
        }

    }
}

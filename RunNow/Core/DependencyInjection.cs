using Microsoft.Extensions.DependencyInjection;
using RunNow.Core;
using RunNow.Services;
using RunNow.ViewModels;
using RunNow.Views;

namespace RunNow.Core
{
    public static class DependencyInjection
    {
        public static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // ✅ NavigationStore 등록
            services.AddSingleton<NavigationStore>();

            // ✅ 비즈니스 로직 서비스
          
            services.AddSingleton<IAuthService, AuthService>();
            //services.AddSingleton<IAuthService>();
            services.AddSingleton<ScenarioService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<TcpClientService>();

            // ✅ ViewModel 등록
            services.AddSingleton<MainWindowViewModel>();  // 네비게이션 담당
            services.AddSingleton<MainViewModel>();        // 홈 화면
            services.AddSingleton<ChatBotViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<CareerAnalysisViewModel>();
            services.AddTransient<FinanceAnalysisViewModel>();
            services.AddTransient<RegisterViewModel>();
            services.AddTransient<ResumeManageViewModel>();
            services.AddTransient<DeepTestViewModel>();
            services.AddTransient<EmotionViewModel>();
            services.AddTransient<EmotionResultViewModel>();
            services.AddTransient<DeepResultViewModel>();


            // ✅ MainWindow
            services.AddSingleton<MainWindow>(sp =>
            {
                var vm = sp.GetRequiredService<MainWindowViewModel>();
                return new MainWindow { DataContext = vm };
            });

            return services.BuildServiceProvider();
        }
    }
}

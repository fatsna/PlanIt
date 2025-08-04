using Microsoft.Extensions.DependencyInjection;
using RunNow.Services;
using RunNow.ViewModels;

namespace RunNow.Core
{
    public static class DependencyInjection
    {
        public static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // 비즈니스 로직 서비스 등록
            services.AddSingleton<ScenarioService>();

            // ViewModel 등록
            services.AddTransient<ChatBotViewModel>();

            return services.BuildServiceProvider();
        }
    }
}

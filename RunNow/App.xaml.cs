using System.Configuration;
using System.Data;
using System.Windows;
<<<<<<< HEAD
using System.Windows.Navigation;
using Microsoft.Extensions.DependencyInjection;
using RunNow.Services;
using RunNow.ViewModels;
=======

>>>>>>> develop
namespace RunNow
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
<<<<<<< HEAD
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();

            services.AddSingleton<TCP_IP_Service>();
            //services.AddSingleton<INavigationService, NavigationService>();
            services.AddTransient<EmotionViewModel>();
            services.AddTransient<EmotionResultViewModel>();

            ServiceProvider = services.BuildServiceProvider();

            base.OnStartup(e);
        }
=======
>>>>>>> develop
    }

}

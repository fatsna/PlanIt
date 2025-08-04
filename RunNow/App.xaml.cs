using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using RunNow.Core;

namespace RunNow
{
    public partial class App : Application
    {
        public static ServiceProvider Services { get; private set; }

        public App()
        {
            Services = DependencyInjection.ConfigureServices();
        }
    }
}

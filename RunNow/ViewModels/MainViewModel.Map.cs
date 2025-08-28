using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using RunNow.Core;
using System;

namespace RunNow.ViewModels
{
    public partial class MainViewModel
    {
        private readonly NavigationStore _nav;
        private readonly IServiceProvider _sp;

        public MainViewModel(NavigationStore nav, IServiceProvider sp)
        {
            _nav = nav;
            _sp = sp;
        }

        [RelayCommand]
        private void Map()
        {
            var vm = _sp.GetRequiredService<MapViewModel>();
            vm.KakaoApiKey = "39a3e2c812a3704cca92838cbfc76646";
            vm.CenterLat = 37.5665;
            vm.CenterLng = 126.9780;
            vm.ZoomLevel = 3;

            _nav.CurrentViewModel = vm;
        }
    }
}

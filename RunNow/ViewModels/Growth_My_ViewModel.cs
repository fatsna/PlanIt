using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using RunNow.Core;
using RunNow.Services;

namespace RunNow.ViewModels
{
    public partial class Growth_My_ViewModel : ObservableObject
    {
        public Growth_My_ViewModel(
            NavigationStore navigationStore,
            IAuthService authService,
            ShareDataService shareDataService,
            IServiceProvider serviceProvider)
        {
            this._navigationStore = navigationStore;
            this._authService = authService;
            this.shareDataService = shareDataService;
            this._serviceProvider = serviceProvider;
        }

        private readonly NavigationStore _navigationStore; // 메인에게 화면넘겨! 객체
        private readonly IAuthService _authService;         // 서버와 통신 객체
        public ShareDataService shareDataService { get; set; }    // 유저 데이터 객체
        private readonly IServiceProvider _serviceProvider;   // 서비스 프로바이더 객체

        [RelayCommand] private void GoTocheck() // 플래너 채우기로
        {
            this._navigationStore.CurrentViewModel = 
                this._serviceProvider.GetRequiredService<growth_check_ViewModel>();
        }
        [RelayCommand] private void Back()
        {
            // 무슨방법이 올바른가?

            //_navigationStore.CurrentViewModel = App.Services.GetRequiredService<MainViewModel>();

            // 이게 MVVM 패턴에 올바르다!
            this._navigationStore.CurrentViewModel = this._serviceProvider.GetRequiredService<Growth_main_ViewModel>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            this.TargetJob = this.shareDataService.wantjob;
            this.DaysLeft = this.shareDataService.period;
            this.setting_ui();

            Console.WriteLine($"dddddddd{this.Progress} {this.NextGoal}");

        }

        private readonly NavigationStore _navigationStore; // 메인에게 화면넘겨! 객체
        private readonly IAuthService _authService;         // 서버와 통신 객체
        public ShareDataService shareDataService { get; set; }    // 유저 데이터 객체
        private readonly IServiceProvider _serviceProvider;   // 서비스 프로바이더 객체

        private void setting_ui()
        {
            foreach (var Goal in this.shareDataService.Goals)
            {
                if(Goal.Date != "")
                {
                    this.Progress += Goal.Goal_Progress;
                    Console.WriteLine($"ddddddddddddddddddddd{this.Progress} {Goal.Goal_Progress}ddddddddddddddddddddddd");
                }
            }
            foreach (var Goal in this.shareDataService.Goals)
            {
                if(Goal.Date == "")
                {
                    this.NextGoal = Goal.Goal;
                }
            }

        }
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
            this.shareDataService.IsDetailVisible = true; // 뒤로가기 메시지 보이기
        }

        // 바인딩 대상 속성들
        [ObservableProperty] private string targetJob = "";
        [ObservableProperty] private int daysLeft = 0;
        [ObservableProperty] private double progress = 0; // 0~100 혹은 0~1, 컨트롤 스펙에 맞춰서
        [ObservableProperty] private string nextGoal = ""; // 다음목표

             // 예: 보조 메트릭 (문자열 배열/리스트)
        [ObservableProperty]
        private string[] subMetrics = new[] {
            "기술 30%", "자격증 10%", "경험 5%"
        };

            // Loaded 시 해야 할 초기화/애니메이션 시작 등
        [RelayCommand] private void CircularProgressLoaded()
        {
            // 예: Progress 초기값/애니메이션 트리거
            this.Progress *= 0.01; // 백으로 나누기!
            Console.WriteLine($"dddddddd{this.Progress} {this.NextGoal}");
        }
    }
}

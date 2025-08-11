using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;  // ✅ 추가: 메시징으로 자동 전환
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using RunNow.Core;

using RunNow.Views;
using System;
using System.Diagnostics;
using System.Windows.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;


namespace RunNow.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly NavigationStore _navigationStore;
        private readonly IServiceProvider _serviceProvider;
        private ShareDataService _shareDataService { get; set; }
        [ObservableProperty]
        private object currentViewModel;

        public MainWindowViewModel(NavigationStore navigationStore, IServiceProvider serviceProvider, ShareDataService shareDataService)
        {
            _navigationStore = navigationStore;
            _serviceProvider = serviceProvider;
            this._shareDataService = shareDataService;
            _navigationStore.CurrentViewModelChanged += () =>
            {
                CurrentViewModel = _navigationStore.CurrentViewModel;
            };

            // 초기 화면: Login
            _navigationStore.CurrentViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
            //_navigationStore.CurrentViewModel = _serviceProvider.GetRequiredService<MainViewModel>();

            CurrentViewModel = _navigationStore.CurrentViewModel;

            //  감정분석 완료 시 결과 화면으로 자동 이동
            //EmotionViewModel.OnQuestionAnswered()에서 "EmotionSurveyCompleted" 토큰으로 JObject를 보낸다고 가정
            WeakReferenceMessenger.Default.Register<ValueChangedMessage<JObject>, string>(
                this,
                "EmotionSurveyCompleted",
                (r, m) =>
                {
                    // DI가 JObject를 주입할 수 없으므로 직접 생성해서 전환
                    var resultVm = new EmotionResultViewModel(m.Value);
                    _navigationStore.CurrentViewModel = resultVm;
                    OnPropertyChanged(nameof(CurrentViewModel));
                });
        }

        [RelayCommand]
        private void GoHome()
        {
            _navigationStore.CurrentViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
        }

        [RelayCommand]
        private void ChatBot()
        {
            _navigationStore.CurrentViewModel = _serviceProvider.GetRequiredService<ChatBotViewModel>();
        }

        [RelayCommand]
        private void CareerAnalysis()
        {
            _navigationStore.CurrentViewModel = _serviceProvider.GetRequiredService<CareerAnalysisViewModel>();
        }

        [RelayCommand]
        private void FinanceAnalysis()
        {
            _navigationStore.CurrentViewModel = _serviceProvider.GetRequiredService<FinanceAnalysisViewModel>();
        }

        [RelayCommand]
        private void QuickTest()
        {
            _navigationStore.CurrentViewModel = _serviceProvider.GetRequiredService<DeepTestViewModel>();
        }

        [RelayCommand]
        private void EmotionAnalysis()
        {
            _navigationStore.CurrentViewModel = _serviceProvider.GetRequiredService<EmotionViewModel>();
        }

        [RelayCommand]
        private async Task ResumeManage()
        {

            _navigationStore.CurrentViewModel = _serviceProvider.GetRequiredService<ResumeManageViewModel>();
        }


        [RelayCommand]
        private void Work24()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.work24.go.kr/cm/main.do",
                UseShellExecute = true
            });
        }


        [RelayCommand]
        private void Planit()
        {
            this._navigationStore.CurrentViewModel = this._serviceProvider.GetRequiredService<Growth_main_ViewModel>();
        }

        // 이전페이지로 돌아가기 커맨드함수
        [RelayCommand]
        private void Goback()
        {
            Console.WriteLine("뒤로가기?");
            this._navigationStore.CurrentViewModel = this._navigationStore.previousViewModel;
            this._shareDataService.IsDetailVisible = false; // 이전페이지로가고 다시 false로 변경
        }


        [RelayCommand]
        private void MsgClose()
        {
            this._shareDataService.IsDetailVisible = false; // 뒤로가기 메시지 닫기
        }

        // ✅ 마이페이지 이동: 진입 전에 서버 데이터 로드
        [RelayCommand]
        private async Task MyPage()
        {
            var vm = _serviceProvider.GetRequiredService<MyPageViewModel>();

            string? uid = _shareDataService?.CurrentUserId; // 로그인 시 저장
            await vm.InitializeAsync(uid);
            _navigationStore.CurrentViewModel = vm;

            OnPropertyChanged(nameof(CurrentViewModel));
        }

        //  결과 화면으로 이동 (수동 트리거용; 파라미터에 JObject를 받으면 그걸로 이동)
        [RelayCommand]
        private void EmotionResult(object? param)
        {
            // 버튼에서 CommandParameter로 JObject를 넘기거나,
            // EmotionViewModel에서 BuildEmotionResultData()로 만든 JObject를 넘길 수 있음.
            if (param is JObject data)
            {
                var vm = new EmotionResultViewModel(data);         // ✅ 직접 생성 (DI 우회)
                _navigationStore.CurrentViewModel = vm;
                OnPropertyChanged(nameof(CurrentViewModel));
                return;
            }

            // 파라미터가 없으면 아무 것도 하지 않음(예외 방지)
            Debug.WriteLine("EmotionResultCommand called without JObject parameter. Ignored.");
        }

    }
}

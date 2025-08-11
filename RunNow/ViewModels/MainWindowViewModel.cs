
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using RunNow.Core;

using RunNow.Views;
using System;
using System.Diagnostics;
using System.Threading.Tasks; // ← 추가
using System.Windows.Threading;

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
            var vm = _serviceProvider.GetRequiredService<ResumeManageViewModel>();
            _navigationStore.CurrentViewModel = vm;
            await Task.Yield();
            await vm.LoadResumeFromServerAsync();   // 이제 정상 컴파일
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

        [RelayCommand] private void Planit()
        {
            this._navigationStore.CurrentViewModel = this._serviceProvider.GetRequiredService<Growth_main_ViewModel>();
        }
        // 이전페이지로 돌아가기 커맨드함수
        [RelayCommand] private void Goback()
        {
            Console.WriteLine("뒤로가기?");
            this._navigationStore.CurrentViewModel = this._navigationStore.previousViewModel;
            this._shareDataService.IsDetailVisible = false; // 이전페이지로가고 다시 false로 변경
        }
        [RelayCommand] private void MsgClose()
        {
            this._shareDataService.IsDetailVisible = false; // 뒤로가기 메시지 닫기
        }
    }
}

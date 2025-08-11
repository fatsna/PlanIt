
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
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

        [ObservableProperty]
        private object currentViewModel;

        public MainWindowViewModel(NavigationStore navigationStore, IServiceProvider serviceProvider)
        {
            _navigationStore = navigationStore;
            _serviceProvider = serviceProvider;


            _navigationStore.CurrentViewModelChanged += () =>
            {
                CurrentViewModel = _navigationStore.CurrentViewModel;
            };

            // 초기 화면: Login
            //_navigationStore.CurrentViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            _navigationStore.CurrentViewModel = _serviceProvider.GetRequiredService <LoginViewModel>();
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

    }
}

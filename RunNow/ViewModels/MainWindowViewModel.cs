
﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using RunNow.Core;
using System;
using System.Diagnostics;

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

            //_navigationStore.CurrentViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
            _navigationStore.CurrentViewModel = _serviceProvider.GetRequiredService<MainViewModel>();

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
        private void ResumeManage()
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

        [RelayCommand] private void Planit()
        {
            this._navigationStore.CurrentViewModel = this._serviceProvider.GetRequiredService<Growth_main_ViewModel>();
        }

    }
}

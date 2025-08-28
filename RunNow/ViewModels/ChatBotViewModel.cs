using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using RunNow.Models;
using RunNow.Services;

namespace RunNow.ViewModels
{
    public partial class ChatBotViewModel : ObservableObject
    {
        private readonly ScenarioService _scenarioService;
        private ScenarioStep? _currentStep;

        [ObservableProperty]
        private string userInput = string.Empty;

        public ObservableCollection<ChatMessage> Messages { get; } = new();
        public ObservableCollection<string> Options { get; } = new();

        public ChatBotViewModel(ScenarioService scenarioService)
        {
            _scenarioService = scenarioService;
            LoadStep("start"); // 첫 스텝 로드
        }

        private void LoadStep(string stepId)
        {
            _currentStep = _scenarioService.GetStep(stepId);
            if (_currentStep == null) return;

            Messages.Add(new ChatMessage
            {
                Sender = "Bot",
                Message = _currentStep.Question,
                ProfileImage = "/Assets/bot.png"
            });

            Options.Clear();
            foreach (var option in _currentStep.Options)
                Options.Add(option);
        }

        [RelayCommand]
        private void Send()
        {
            if (string.IsNullOrWhiteSpace(UserInput)) return;

            Messages.Add(new ChatMessage { Sender = "User", Message = UserInput, ProfileImage = "/Assets/user.png" });

            var nextId = _scenarioService.MatchUserInput(_currentStep, UserInput);
            if (nextId != null)
                LoadStep(nextId);
            else
                Messages.Add(new ChatMessage { Sender = "Bot", Message = "해당 명령을 찾을 수 없습니다. 옵션을 선택해주세요.", ProfileImage = "/Assets/bot.png" });

            UserInput = string.Empty;
        }


        [RelayCommand]
        private void SelectOption(string option)
        {
            Messages.Add(new ChatMessage { Sender = "User", Message = option, ProfileImage = "/Assets/user.png" });

            if (_currentStep != null && _currentStep.NextStep.ContainsKey(option))
                LoadStep(_currentStep.NextStep[option]);
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using RunNow.Models;
using RunNow.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using RunNow.Core;

namespace RunNow.ViewModels
{
    public partial class EmotionViewModel : ObservableObject
    {
        private readonly TcpClientService _tcpService;

        [ObservableProperty] private ObservableCollection<QuestionModel> allQuestions = new();
        [ObservableProperty] private ObservableCollection<QuestionModel> currentQuestions = new();
        [ObservableProperty] private int currentPageIndex = 0;

        [ObservableProperty] private bool isStep1Visible = true;
        [ObservableProperty] private bool isStep2Visible = false;
        [ObservableProperty] private bool isStep3Visible = false;

        public EmotionViewModel(TcpClientService tcpService)
        {
            _tcpService = tcpService;
            LoadQuestions();
            UpdateCurrentQuestions();
        }

        private void LoadQuestions()
        {
            var categoryGroups = Constants.Questions
                .GroupBy(q => q.Category)
                .ToDictionary(g => g.Key, g => g.ToList());

            Random random = new Random();
            int totalQuestionCount = 10;

            var selectedQuestions = new List<QuestionModel>();

            foreach (var category in categoryGroups.Keys)
            {
                var questionsInCategory = categoryGroups[category];
                var randomQuestion = questionsInCategory[random.Next(questionsInCategory.Count)];

                var q = new QuestionModel
                {
                    QuestionText = $"Q{selectedQuestions.Count + 1}. {randomQuestion.QuestionText}",
                    Category = randomQuestion.Category,
                    IsPositive = randomQuestion.IsPositive
                };

                q.Answered += OnQuestionAnswered;
                selectedQuestions.Add(q);
                questionsInCategory.Remove(randomQuestion);
            }

            var remainingQuestionsPool = categoryGroups.Values.SelectMany(q => q).OrderBy(q => Guid.NewGuid()).ToList();
            for (int i = 0; i < totalQuestionCount - selectedQuestions.Count && i < remainingQuestionsPool.Count; i++)
            {
                var item = remainingQuestionsPool[i];
                var q = new QuestionModel
                {
                    QuestionText = $"Q{selectedQuestions.Count + 1}. {item.QuestionText}",
                    Category = item.Category,
                    IsPositive = item.IsPositive
                };
                q.Answered += OnQuestionAnswered;
                selectedQuestions.Add(q);
            }

            AllQuestions = new ObservableCollection<QuestionModel>(selectedQuestions);
        }

        private void UpdateCurrentQuestions()
        {
            CurrentQuestions.Clear();
            foreach (var q in AllQuestions.Skip(CurrentPageIndex * 5).Take(5))
            {
                CurrentQuestions.Add(q);
            }
        }

        private void OnQuestionAnswered()
        {
            if (CurrentQuestions.All(q => q.SelectedAnswerIndex != -1))
            {
                if ((CurrentPageIndex + 1) * 5 < AllQuestions.Count)
                {
                    CurrentPageIndex++;
                    UpdateCurrentQuestions();
                }
                else
                {
                    ShowResults();
                }
            }
        }

        [RelayCommand]
        private void GoToStep2()
        {
            IsStep1Visible = false;
            IsStep2Visible = true;
        }

        [RelayCommand]
        private void GoToStep3()
        {
            IsStep2Visible = false;
            IsStep3Visible = true;
        }

        [RelayCommand]
        private async void ShowResults()
        {
            var emotionList = CalculateCategoryAverages();

            var payload = new JObject
            {
                ["protocol"] = "100_0",
                ["type"] = "deep_analysis",
                ["emotion"] = emotionList
            };

            await _tcpService.ConnectAsync();
            var response = await _tcpService.SendJsonToServer(payload);

            MessageBox.Show(response?.ToString() ?? "서버 응답 없음", "서버 응답");
        }

        private JArray CalculateCategoryAverages()
        {
            var grouped = AllQuestions.GroupBy(q => q.Category);
            var resultArray = new JArray();

            foreach (var group in grouped)
            {
                var adjusted = group.Select(q =>
                {
                    int score = 5 - q.SelectedAnswerIndex;
                    if (!q.IsPositive) score = 6 - score;
                    return score;
                });

                var categoryObj = new JObject
                {
                    ["category"] = group.Key,
                    ["average"] = adjusted.Average()
                };

                resultArray.Add(categoryObj);
            }

            return resultArray;
        }


        [RelayCommand]
        private void SelectAnswer(Tuple<QuestionModel, string> param)
        {
            if (param == null) return;

            var (question, answer) = param;

            var answers = new List<string> { "매우그렇다", "그렇다", "보통이다", "아니다", "매우아니다" };

            // 선택된 답변의 인덱스를 저장
            question.SelectedAnswerIndex = answers.IndexOf(answer);

            // 이벤트 발생 → 다음 페이지 넘어가기 로직 체크
            //question.Answered?.Invoke();
        }

    }

}

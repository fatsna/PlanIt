using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using RunNow.Core;
using RunNow.Models;
using RunNow.Services;
using static System.Formats.Asn1.AsnWriter;

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

        private readonly int[] indexMapping = { 6, 5, 4, 3, 2, 1, 0 };

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

//<<<<<<< HEAD
            // 최종 결과를 AllQuestions에 세팅
            this.AllQuestions.Clear();
            foreach (var q in selectedQuestions)
            {
                this.AllQuestions.Add(q);
            }
            //AllQuestions = new ObservableCollection<QuestionModel>(selectedQuestions);
//=======
//            AllQuestions = new ObservableCollection<QuestionModel>(selectedQuestions);
//>>>>>>> HJY
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

            Console.WriteLine("A question was answered.");

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
//<<<<<<< HEAD
        private void ShowResults()
        {
            JObject result = CalculateCategoryAverages();
            // 여기서 결과로가는 로직추가해야함
            MessageBox.Show(string.Join("\n", result.Properties().Select(p => $"{p.Name}: {p.Value:F2}점")), "결과");
        }

        private JObject CalculateCategoryAverages()
        {
            var grouped = AllQuestions.GroupBy(q => q.Category);
            var result = new JObject();
            JArray categoryNames = new JArray();
            JArray scores = new JArray();
            JArray answer = new JArray();
            JArray scores100 = new JArray();
//=======
//        private async void ShowResults()
//        {
//            var emotionList = CalculateCategoryAverages();

//            var payload = new JObject
//            {
//                ["protocol"] = "100_0",
//                ["type"] = "deep_analysis",
//                ["emotion"] = emotionList
//            };

//            await _tcpService.ConnectAsync();
//            var response = await _tcpService.SendJsonToServer(payload);

//            MessageBox.Show(response?.ToString() ?? "서버 응답 없음", "서버 응답");
//        }

//        private JArray CalculateCategoryAverages()
//        {
//            var grouped = AllQuestions.GroupBy(q => q.Category);
//            var resultArray = new JArray();
//>>>>>>> HJY

            foreach (var group in grouped)
            {
                var adjusted = group.Select(q =>
                {
                    int score = 5 - q.SelectedAnswerIndex;
                    if (!q.IsPositive) score = 6 - score;
                    return score;
                });
//<<<<<<< HEAD
                result[group.Key] = adjusted.Average();
            }
            double step = 100.0 / 7.0;  // 7구간 점수 폭 (14.2857)

            for (int i = 0; i < categoryNames.Count; i++)
            {
                // 1. 점수를 정수로 변환 (반올림)
                int score = (int)Math.Round((double)scores[i] * 20);  // 예: 3.75점 → 75점

                // 2. 구간 인덱스 계산
                int resultIndex = score / (int)step;  // 정수로 나눔 (14점씩 구간)
                resultIndex = Math.Clamp(resultIndex, 0, 6);

                // 3. 매핑 테이블 적용 (뒤집기)
                if ((categoryNames[i].ToString() != "감정 상태")
                    && (categoryNames[i].ToString() != "회복탄력성 / 에너지 상태")
                    && (categoryNames[i].ToString() != "자기정체감/불확실성 상태"))
                {
                    resultIndex = this.indexMapping[resultIndex];
                }
                scores100.Add(score);
                answer.Add(Constants.categori_result[i][resultIndex]);
            }


            JObject jsonData = new JObject
            {
                ["Categorys"] = categoryNames,
                ["Scores"] = scores,
                ["answer"] = answer,
                ["Scores100"] = scores100
            };
            return result;
        }

        [RelayCommand]
        private void SelectAnswer(Tuple<QuestionModel, string> param)
        {
            Console.WriteLine("으아아ㅏ");
            var question = param.Item1; // 모델
            var answerText = param.Item2; // 문자열
            int index = question.Answers.IndexOf(answerText);
            question.SelectedAnswerIndex = index;

            Console.WriteLine($"[DEBUG] 선택된 질문: {question.QuestionText}");
            Console.WriteLine($"[DEBUG] 선택된 답변: {answerText} (index {index})");

            if (param == null) return;

            //var (question, answer) = param;
            var answers = new List<string> { "매우그렇다", "그렇다", "보통이다", "아니다", "매우아니다" };

 
            // 선택된 답변의 인덱스를 저장
            question.SelectedAnswerIndex = answers.IndexOf(answerText);
//=======

//                var categoryObj = new JObject
//                {
//                    ["category"] = group.Key,
//                    ["average"] = adjusted.Average()
//                };

//                resultArray.Add(categoryObj);
//            }

//            return resultArray;
//        }


//        [RelayCommand]
//        private void SelectAnswer(Tuple<QuestionModel, string> param)
//        {
//            if (param == null) return;

//            var (question, answer) = param;

//            var answers = new List<string> { "매우그렇다", "그렇다", "보통이다", "아니다", "매우아니다" };

//            // 선택된 답변의 인덱스를 저장
//            question.SelectedAnswerIndex = answers.IndexOf(answer);
//>>>>>>> HJY

            // 이벤트 발생 → 다음 페이지 넘어가기 로직 체크
            //question.Answered?.Invoke();
        }

    }

}

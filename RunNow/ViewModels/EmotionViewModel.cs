using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RunNow.Core;
using RunNow.Models;
using RunNow.Services;

namespace RunNow.ViewModels
{
    public class EmotionViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<QuestionModel> AllQuestions { get; set; } = new ObservableCollection<QuestionModel>();
        public ObservableCollection<QuestionModel> CurrentQuestions { get; set; } = new ObservableCollection<QuestionModel>();
        // 결과를 보여줄 때 사용할 Action
        public Action<JObject> Result;

        private int currentPageIndex = 0;

        public EmotionViewModel(Constants.Enum_emotion check)
        {
            Console.WriteLine($"{check}Initializing EmotionViewModel...");
            this.SelectAnswerCommand = new RelayCommand<object>(OnSelectAnswer);
            //LoadQuestions(check);
            this.LoadQuestions();
            this.UpdateCurrentQuestions();
            Console.WriteLine("EmotionViewModel initialized with questions.");
        }

        private void LoadQuestions()
        {
            Console.WriteLine("LoadQuestions EmotionViewModel...");

            // 1. 카테고리별로 그룹화
            var categoryGroups = Constants.Questions
                .GroupBy(q => q.Category)
                .ToDictionary(g => g.Key, g => g.ToList());

            Random random = new Random();
            int totalQuestionCount = 10;
            int categoryCount = categoryGroups.Count;

            // 2. 각 카테고리당 최소 1개씩 뽑기
            List<QuestionModel> selectedQuestions = new List<QuestionModel>();

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

                q.Answered += this.OnQuestionAnswered;
                selectedQuestions.Add(q);

                // 뽑은 질문은 리스트에서 제거 (중복 방지)
                questionsInCategory.Remove(randomQuestion);
            }

            // 3. 남은 (10 - 카테고리 수) 만큼 랜덤하게 뽑기
            int remainingCount = totalQuestionCount - selectedQuestions.Count;

            // 카테고리별 남은 질문을 전부 모아서 하나의 풀로 만듦
            var remainingQuestionsPool = categoryGroups.Values.SelectMany(q => q).ToList();

            // 셔플해서 중복 없이 추가
            var shuffledRemaining = remainingQuestionsPool.OrderBy(q => Guid.NewGuid()).ToList();
            for (int i = 0; i < remainingCount && i < shuffledRemaining.Count; i++)
            {
                var item = shuffledRemaining[i];

                var q = new QuestionModel
                {
                    QuestionText = $"Q{selectedQuestions.Count + 1}. {item.QuestionText}",
                    Category = item.Category,
                    IsPositive = item.IsPositive
                };

                q.Answered += this.OnQuestionAnswered;
                selectedQuestions.Add(q);
            }

            // 최종 결과를 AllQuestions에 세팅
            this.AllQuestions.Clear();
            foreach (var q in selectedQuestions)
            {
                this.AllQuestions.Add(q);
            }
        }

        //private void LoadQuestions(Constants.Enum_emotion check)
        //{
        //    Console.WriteLine($"{check}LoadQuestions EmotionViewModel...");

        //    Console.WriteLine("Loading questions...");

        //    // 1. 카테고리별로 그룹화
        //    var categoryGroups = Constants.Questions
        //        .GroupBy(q => q.Category)
        //        .ToDictionary(g => g.Key, g => g.ToList());

        //    Random random = new Random();
        //    int questionsPerCategory = Convert.ToInt16(check); // 테스트인지 구분

        //    foreach (var category in categoryGroups.Keys)
        //    {
        //        var questionsInCategory = categoryGroups[category];

        //        // 2. 셔플
        //        var shuffled = questionsInCategory.OrderBy(q => Guid.NewGuid()).ToList();

        //        // 3. 최대 questionsPerCategory개 뽑기
        //        int pickCount = Math.Min(questionsPerCategory, shuffled.Count);

        //        for (int i = 0; i < pickCount; i++)
        //        {
        //            var item = shuffled[i];

        //            var q = new QuestionModel
        //            {
        //                QuestionText = $"Q{AllQuestions.Count + 1}. {item.QuestionText}",
        //                Category = item.Category,
        //                IsPositive = item.IsPositive
        //            };

        //            q.Answered += this.OnQuestionAnswered;
        //            this.AllQuestions.Add(q);
        //        }
        //    }
        //}

        private void OnQuestionAnswered()
        {
            Console.WriteLine("A question was answered.");
            if (this.CurrentQuestions.All(q => q.SelectedAnswerIndex != -1))
            {
                if ((currentPageIndex + 1) * 5 < this.AllQuestions.Count)
                {
                    currentPageIndex++;
                    UpdateCurrentQuestions();
                }
                else
                {
                    ShowResults();
                }
            }
        }

        private void UpdateCurrentQuestions()
        {
            this.CurrentQuestions.Clear();
            foreach (var q in this.AllQuestions.Skip(currentPageIndex * 5).Take(5))
            {
                this.CurrentQuestions.Add(q);
            }
        }

        private void ShowResults()
        {
            Console.WriteLine("Calculating category averages...");
            JObject data = new JObject();
            data["emotion_result"] = new JArray();
            JObject result = this.CalculateCategoryAverages();
            // 딕셔너리 형태로 결과출력방법
            //string message = string.Join("\n", result.Select(kv => $"{kv.Key}: {kv.Value:F2}점"));

            // JSON 형태로 결과출력방법
            string message = string.Join("\n", result.Properties()
                                                     .Select(p => $"{p.Name}: {p.Value:F2}점"));
            Console.WriteLine(message);
            MessageBox.Show(message, "결과");
            // 여기서 메인으로 돌아가서 감정결과를 보여주기?
            
            MessengerService.Send(result); // resultModel: Emotion_result_model

            //this.Result?.Invoke(result);
        }
         private readonly int[] indexMapping = { 6, 5, 4, 3, 2, 1, 0 };

        public JObject CalculateCategoryAverages()
        {
            Console.WriteLine("Calculating category averages...");
            var grouped = this.AllQuestions.GroupBy(q => q.Category);
            var result = new Dictionary<string, double>();
            JArray categoryNames = new JArray();
            JArray scores = new JArray();
            JArray answer = new JArray();
            JArray scores100 = new JArray();

            foreach (var group in grouped)
            {
                var adjusted = group.Select(q =>
                {
                    int score = 5 - q.SelectedAnswerIndex;  // 매우그렇다(0) ~ 매우아니다(4) → 5~1점
                    if (!q.IsPositive) score = 6 - score;   // 부정문항 반전
                    return score;
                });
                categoryNames.Add(group.Key);
                scores.Add(adjusted.Average());
       
                result[group.Key] = adjusted.Average();
                Console.WriteLine($"Category: {group.Key}, Average Score: {result[group.Key]}");
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

            return jsonData;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ICommand SelectAnswerCommand { get; }

        private void OnSelectAnswer(object param)
        {
            if (param is Tuple<QuestionModel, string> tuple)
            {
                var question = tuple.Item1; // 모델
                var answerText = tuple.Item2; // 문자열

                int index = question.Answers.IndexOf(answerText);
                question.SelectedAnswerIndex = index;

                Console.WriteLine($"[DEBUG] 선택된 질문: {question.QuestionText}");
                Console.WriteLine($"[DEBUG] 선택된 답변: {answerText} (index {index})");
            }
            else
            {
                Console.WriteLine("[ERROR] CommandParameter 파싱 실패");
            }
        }
    }

}

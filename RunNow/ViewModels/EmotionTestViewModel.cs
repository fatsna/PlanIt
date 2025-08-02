using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using RunNow.Models;
using RunNow.Core;

namespace RunNow.ViewModels
{
    public class EmotionTestViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<QuestionModel> AllQuestions { get; set; } = new ObservableCollection<QuestionModel>();
        public ObservableCollection<QuestionModel> CurrentQuestions { get; set; } = new ObservableCollection<QuestionModel>();

        private int currentPageIndex = 0;

        public EmotionTestViewModel()
        {
            SelectAnswerCommand = new RelayCommand<object>(OnSelectAnswer);
            LoadQuestions();
            UpdateCurrentQuestions();
            Console.WriteLine("EmotionTestViewModel initialized with questions.");
        }

        private void LoadQuestions()
        {
            Console.WriteLine("Loading questions...");

            for (int i = 0; i < Constants.Questions.Count; i++)
            {
                var item = Constants.Questions[i];

                var q = new QuestionModel
                {
                    QuestionText = $"Q{i + 1}. {item.QuestionText}",
                    Category = item.Category,
                    IsPositive = item.IsPositive
                };

                q.Answered += OnQuestionAnswered;
                AllQuestions.Add(q);
            }
        }

        private void OnQuestionAnswered()
        {
            Console.WriteLine("A question was answered.");
            if (CurrentQuestions.All(q => q.SelectedAnswerIndex != -1))
            {
                if ((currentPageIndex + 1) * 5 < AllQuestions.Count)
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
            CurrentQuestions.Clear();
            foreach (var q in AllQuestions.Skip(currentPageIndex * 5).Take(5))
            {
                CurrentQuestions.Add(q);
            }
        }

        private void ShowResults()
        {
            Console.WriteLine("Calculating category averages...");
            var result = CalculateCategoryAverages();
            string message = string.Join("\n", result.Select(kv => $"{kv.Key}: {kv.Value:F2}점"));
            MessageBox.Show(message, "결과");
        }

        public Dictionary<string, double> CalculateCategoryAverages()
        {
            Console.WriteLine("Calculating category averages...");
            var grouped = AllQuestions.GroupBy(q => q.Category);
            var result = new Dictionary<string, double>();

            foreach (var group in grouped)
            {
                var adjusted = group.Select(q =>
                {
                    int score = 5 - q.SelectedAnswerIndex;  // 매우그렇다(0) ~ 매우아니다(4) → 5~1점
                    if (!q.IsPositive) score = 6 - score;   // 부정문항 반전
                    return score;
                });

                result[group.Key] = adjusted.Average();
            }

            return result;
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
                var question = tuple.Item1;
                var answerText = tuple.Item2;

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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RunNow.Models
{
    public partial class QuestionModel : ObservableObject
    {
        public string Id { get; set; }
        public string QuestionText { get; set; }
        public string Category { get; set; }
        public bool IsPositive { get; set; }

        public List<string> Answers { get; } = new() { "매우그렇다", "그렇다", "보통이다", "아니다", "매우아니다" };

        [ObservableProperty]
        private int selectedAnswerIndex = -1;

        public event Action Answered;

        partial void OnSelectedAnswerIndexChanged(int value)
        {
            Answered?.Invoke();
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;


namespace RunNow.Models
{
    public partial class QuestionModel : ObservableObject
    {
        public string Id { get; set; }
        public string QuestionText { get; set; }
        public string Category { get; set; }
        public bool IsPositive { get; set; }

        [ObservableProperty]
        private int selectedAnswerIndex = -1;

        public event Action Answered;

        partial void OnSelectedAnswerIndexChanged(int value)
        {
            Answered?.Invoke();
        }
    }
}

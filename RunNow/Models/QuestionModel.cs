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

        // 간단하게 위의 코드로 대체
        //private int _selectedAnswerIndex = -1;
        //public int SelectedAnswerIndex
        //{
        //    get => this._selectedAnswerIndex;
        //    set
        //    {
        //        if (this._selectedAnswerIndex != value)
        //        {
        //            this._selectedAnswerIndex = value;
        //            OnPropertyChanged(nameof(SelectedAnswerIndex));
        //            Answered?.Invoke();
        //        }
        //    }
        //}

        //public event PropertyChangedEventHandler PropertyChanged;
        //protected void OnPropertyChanged(string propName)
        //{
        //    Console.WriteLine($"Property changed: {propName}");
        //    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        //}
    }
}

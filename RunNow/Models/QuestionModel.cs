using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunNow.Models
{
    public class QuestionModel : INotifyPropertyChanged
    {
        public string QuestionText { get; set; }
        public string Category { get; set; }
        public bool IsPositive { get; set; }
        public List<string> Answers { get; set; } = new List<string> { "매우그렇다", "그렇다", "보통이다", "아니다", "매우아니다" };

        public event Action Answered;

        private int _selectedAnswerIndex = -1;
        public int SelectedAnswerIndex
        {
            get => _selectedAnswerIndex;
            set
            {
                if (_selectedAnswerIndex != value)
                {
                    _selectedAnswerIndex = value;
                    OnPropertyChanged(nameof(SelectedAnswerIndex));
                    Answered?.Invoke();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
        {
            Console.WriteLine($"Property changed: {propName}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }

}

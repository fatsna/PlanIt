using System.Collections.Generic;
using System.Windows.Media;

namespace RunNow.Models
{
    internal class Emotion_result_model
    {
        public List<string> Categorys { get; set; } = new();
        public List<string> answer { get; set; } = new();
        public List<int> Scores100 { get; set; } = new();
    }

    public class EmotionResultItem
    {
        public string Category { get; set; }
        public int Score { get; set; }   // 0 ~ 100
        public string Description { get; set; }

        private static readonly SolidColorBrush WorstBrush = new SolidColorBrush(Color.FromRgb(0xEF, 0x53, 0x50)); // #EF5350
        private static readonly SolidColorBrush BadBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xA7, 0x26)); // #FFA726
        private static readonly SolidColorBrush MidBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xCA, 0x28)); // #FFCA28
        private static readonly SolidColorBrush GoodBrush = new SolidColorBrush(Color.FromRgb(0x66, 0xBB, 0x6A)); // #66BB6A

        public Brush BarColor => Score switch
        {
            <= 25 => WorstBrush,
            <= 50 => BadBrush,
            <= 75 => MidBrush,
            _ => GoodBrush
        };

        public double BarWidth => Score * 2.5;
    }
}
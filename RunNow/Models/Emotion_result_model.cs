using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static System.Formats.Asn1.AsnWriter;

namespace RunNow.Models
{
    internal class Emotion_result_model
    {
        public List<string> Categorys { get; set; }
        public List<string> answer { get; set; }
        public List<int> Scores100 { get; set; }
    }
    public class EmotionResultItem
    {
        public string Category { get; set; }
        public int Score { get; set; }  // 0 ~ 100
        public string Description { get; set; }

        public Brush BarColor => Score switch
        {
            <= 25 => Brushes.Red,
            <= 50 => Brushes.Orange,
            <= 75 => Brushes.Gold,
            _ => Brushes.LimeGreen
        };

        public double BarWidth => Score * 2.5;  // 예: 100점이면 250px 너비
    }
}

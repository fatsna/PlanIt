using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json.Linq;
using RunNow.Models;
using System.Collections.ObjectModel;

public partial class EmotionResultViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<EmotionResultItem> emotionResults = new();

    [ObservableProperty]
    private string randomDiagnosis;

    public EmotionResultViewModel(JObject data)
    {
        Emotion_result_model model = data.ToObject<Emotion_result_model>();

        for (int i = 0; i < model.Categorys.Count; i++)
        {
            EmotionResults.Add(new EmotionResultItem
            {
                Category = model.Categorys[i],
                Score = model.Scores100[i],
                Description = WrapText(model.answer[i]) 
            });
        }

        // 진단 메시지 랜덤 1개 선택
        var rnd = new Random();
        RandomDiagnosis = model.answer[rnd.Next(model.answer.Count)];
    }

    string WrapText(string input, int maxLineLength = 30)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        var words = input.Split(' ');
        var result = new StringBuilder();
        int lineLength = 0;

        foreach (var word in words)
        {
            if (lineLength + word.Length > maxLineLength)
            {
                result.AppendLine();
                lineLength = 0;
            }

            result.Append(word + " ");
            lineLength += word.Length + 1;
        }

        return result.ToString().TrimEnd();
    }
}

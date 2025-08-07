// ✅ ViewModels/DeepResultViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using RunNow.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json.Linq;


namespace RunNow.ViewModels
{
    public partial class DeepResultViewModel : ObservableObject
    {
        [ObservableProperty]
        private string resultText = "분석 결과가 여기에 표시됩니다.";

        [ObservableProperty]
        private ISeries[] series;

        [ObservableProperty]
        private Axis[] xAxes;

        [ObservableProperty]
        private Axis[] yAxes;

        [ObservableProperty]
        private ObservableCollection<RecommendedJobItem> recommendedJobs = new();

        public DeepResultViewModel()
        {
            Series = new ISeries[] { };
            XAxes = new Axis[] { };
            YAxes = new Axis[] { };
        }

        public void SetResultText(string summary)
        {
            ResultText = summary;
        }

        public void SetResults(IEnumerable<EmotionResultItem> emotionResults)
        {
            var values = emotionResults.Select(e => (double)e.Score).ToArray();
            var labels = emotionResults.Select(e => e.Category).ToArray();

            Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name = "카테고리별 평균 점수",
                    Values = values,
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsSize = 14,
                    DataLabelsPosition = DataLabelsPosition.Top
                }
            };

            XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = labels,
                    LabelsRotation = 15,
                    TextSize = 14,
                    LabelsPaint = new SolidColorPaint
                    {
                        SKTypeface = SKTypeface.FromFamilyName("맑은 고딕", SKFontStyle.Normal),
                        Color = SKColors.Black // 폰트 색상
                    }
                }
            };

            YAxes = new Axis[]
            {
                new Axis
                {
                    MinLimit = 1,
                    MaxLimit = 5,
                    TextSize = 14,
                    LabelsPaint = new SolidColorPaint
                    {
                        SKTypeface = SKTypeface.FromFamilyName("맑은 고딕", SKFontStyle.Normal),
                        Color = SKColors.Black
                    }
                }
            };


        }

        //추천 직업 설정
        public void SetRecommendedJobs(JArray jobArray)
        {
            RecommendedJobs.Clear(); // 기존 리스트 지우고
            foreach (var job in jobArray)
            {
                RecommendedJobs.Add(new RecommendedJobItem
                {
                    Job = job["job"]?.ToString() ?? "",
                    Description = job["description"]?.ToString() ?? "",
                    Reason = job["reason"]?.ToString() ?? ""
                });
            }
        }

    }
}

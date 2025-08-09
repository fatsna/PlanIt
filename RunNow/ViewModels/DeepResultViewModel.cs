
﻿// ✅ ViewModels/DeepResultViewModel.cs
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
//        [ObservableProperty]
//        private string resultText = "분석 결과가 여기에 표시됩니다.";

//        [ObservableProperty]
//        private ISeries[] series;

//        [ObservableProperty]
//        private Axis[] xAxes;

//        [ObservableProperty]
//        private Axis[] yAxes;

//        [ObservableProperty]
//        private ObservableCollection<RecommendedJobItem> recommendedJobs = new();
        [ObservableProperty] private string resultText = "분석 결과가 여기에 표시됩니다.";

        [ObservableProperty] private ISeries[] series;
        [ObservableProperty] private Axis[] xAxes;
        [ObservableProperty] private Axis[] yAxes;

        [ObservableProperty] private ObservableCollection<RecommendedJobItem> recommendedJobs = new();

        // ✅ 결과 화면에 보여줄 현실적 조언
        public ObservableCollection<string> RealisticAdvice { get; } = new();

        // ✅ (제너레이터 대신) 일반 프로퍼티로 명시
        public SolidColorPaint? LegendTextPaint { get; set; }
        public SolidColorPaint? TooltipTextPaint { get; set; }
        public SolidColorPaint? TitlePaint { get; set; }
        public string Title { get; set; } = "감정 진단 결과";
        public double TitleTextSize { get; set; } = 18;

        public DeepResultViewModel()
        {
            Series = new ISeries[] { };
            XAxes = new Axis[] { };
            YAxes = new Axis[] { };
            var typeface = SKTypeface.FromFamilyName("Malgun Gothic");

            LegendTextPaint = new SolidColorPaint(SKColors.Black) { SKTypeface = typeface };
            TooltipTextPaint = new SolidColorPaint(SKColors.Black) { SKTypeface = typeface };
            TitlePaint = new SolidColorPaint(SKColors.Black) { SKTypeface = typeface };
        }

        public void SetResultText(string summary) => ResultText = summary;


        public void SetResults(IEnumerable<EmotionResultItem> emotionResults)
        {
            var values = emotionResults.Select(e => (double)e.Score).ToArray();
            var labels = emotionResults.Select(e => e.Category).ToArray();
            var typeface = SKTypeface.FromFamilyName("Malgun Gothic");

            Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name = "카테고리별 평균 점수",
                    Values = values,
//                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black) { SKTypeface = typeface },
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
//                        SKTypeface = SKTypeface.FromFamilyName("맑은 고딕", SKFontStyle.Normal),
//                        Color = SKColors.Black // 폰트 색상
                        SKTypeface = typeface,
                        Color = SKColors.Black
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
//                        SKTypeface = SKTypeface.FromFamilyName("맑은 고딕", SKFontStyle.Normal),
                        SKTypeface = typeface,
                        Color = SKColors.Black
                    }
                }
            };


            // 제목도 같은 폰트 적용
            Title = "감정 진단 결과 (평균 점수)";
            TitleTextSize = 18;
            TitlePaint = new SolidColorPaint(SKColors.Black) { SKTypeface = typeface };
        }

        public void SetRealisticAdvice(JArray arr)
        {
            RealisticAdvice.Clear();
            foreach (var a in arr)
            {
                if (a.Type == JTokenType.String)
                    RealisticAdvice.Add(a.ToString());
                else
                    RealisticAdvice.Add(a["text"]?.ToString() ?? a.ToString());
            }
        }

        public void SetRecommendedJobs(JArray jobArray)
        {
            RecommendedJobs.Clear();

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

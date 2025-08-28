using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Newtonsoft.Json.Linq;
using RunNow.Models;
using SkiaSharp;
using System.Collections.ObjectModel;

using System.Collections.Generic;
using System.Linq;

namespace RunNow.ViewModels
{
    public partial class DeepResultViewModel : ObservableObject
    {



        [ObservableProperty] private string resultText = "분석 결과가 여기에 표시됩니다.";
        [ObservableProperty] private ISeries[] series;
        [ObservableProperty] private Axis[] xAxes;
        [ObservableProperty] private Axis[] yAxes;
        [ObservableProperty] private ObservableCollection<RecommendedJobItem> recommendedJobs = new();

        // 현실적 조언
        public ObservableCollection<string> RealisticAdvice { get; } = new();

        // 그래프 폰트 바인딩용
        public SolidColorPaint LegendTextPaint { get; }
        public SolidColorPaint TitlePaint { get; }          // 그대로 두되 크기 지정 제거
        public SolidColorPaint AxisNamePaint { get; }       // (필요 시 사용)
        public SolidColorPaint AxisLabelsPaint { get; }     // (필요 시 사용)
        public SolidColorPaint TooltipTextPaint { get; }    // 툴팁 글꼴

        public string Title { get; private set; } = "감정 진단 결과";

        public DeepResultViewModel()

        {

            RecommendedJobs.Add(new RecommendedJobItem
            {
                Job = "소셜 미디어 관리자",
                Description = "플랫폼에서 브랜드 온라인 존재감 관리 및 콘텐츠 기획/게시",
                Reason = "커뮤니케이션과 창의적 업무가 강점에 잘 맞음"
            });

            Series = [];
            XAxes = [];
            YAxes = [];

            var typeface = SKTypeface.FromFamilyName("Malgun Gothic");

            // Paint들 초기화 (크기는 해당 객체 쪽 Size 속성 사용)
            LegendTextPaint = new SolidColorPaint(SKColors.Black) { SKTypeface = typeface };
            TooltipTextPaint = new SolidColorPaint(SKColors.Black) { SKTypeface = typeface };
            TitlePaint = new SolidColorPaint(SKColors.Black) { SKTypeface = typeface };
            AxisNamePaint = new SolidColorPaint(SKColors.Black) { SKTypeface = typeface };
            AxisLabelsPaint = new SolidColorPaint(SKColors.Black) { SKTypeface = typeface };

            // 샘플 데이터
            var emotionResults = new List<EmotionResult>
            {
                new EmotionResult { Category = "동기부여", Average = 78 },
                new EmotionResult { Category = "스트레스", Average = 62 },
                new EmotionResult { Category = "가치충돌", Average = 55 },
                new EmotionResult { Category = "자기정체감", Average = 81 },
            };


            var values = emotionResults.Select(e => (double)e.Average).ToArray();
            var labels = emotionResults.Select(e => e.Category).ToArray();

            var tf2 = SKTypeface.FromFamilyName("Malgun Gothic");

            Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name = "카테고리별 평균 점수",
                    Values = values,

                    DataLabelsPaint = new SolidColorPaint(SKColors.Black) { SKTypeface = tf2 },
                    DataLabelsSize = 14, // 크기 지정은 여기서
                    DataLabelsPosition = DataLabelsPosition.Top
                }
            };

            XAxes = new[]
            {
                new Axis
                {

                    Name = "카테고리",
                    NamePaint   = new SolidColorPaint(SKColors.Black) { SKTypeface = tf2 },
                    LabelsPaint = new SolidColorPaint(SKColors.Black) { SKTypeface = tf2 },
                    Labels = labels
                }
            };

            YAxes = new[]
            {
                new Axis
                {
                    Name = "점수",
                    NamePaint   = new SolidColorPaint(SKColors.Black) { SKTypeface = tf2 },
                    LabelsPaint = new SolidColorPaint(SKColors.Black) { SKTypeface = tf2 }
                }
            };


            Title = "감정 진단 결과 (평균 점수)";
            OnPropertyChanged(nameof(Title));
        }

        public void ApplyResultText(string summary) => ResultText = summary;

        public void SetRecommendedJobs(JArray jobArray)
        {
            RecommendedJobs.Clear();

            if (jobArray == null) return;

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

        // DeepTestViewModel에서 호출하는 메서드 3개 추가

        //  결과 아이템을 받아 차트/축/제목 세팅
        public void SetResults(IList<EmotionResultItem> items)
        {
            if (items == null || items.Count == 0)
            {
                Series = [];
                XAxes = [];
                YAxes = [];
                return;
            }

            var values = items.Select(i => (double)i.Score).ToArray();
            var labels = items.Select(i => i.Category ?? string.Empty).ToArray();
            var tf = SKTypeface.FromFamilyName("Malgun Gothic");

            Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name = "카테고리별 평균 점수",
                    Values = values,
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black) { SKTypeface = tf },
                    DataLabelsSize = 14,
                    DataLabelsPosition = DataLabelsPosition.Top
                }
            };

            XAxes = new[]
            {
                new Axis
                {
                    Name = "카테고리",
                    Labels = labels,
                    NamePaint   = new SolidColorPaint(SKColors.Black) { SKTypeface = tf },
                    LabelsPaint = new SolidColorPaint(SKColors.Black) { SKTypeface = tf }
                }
            };

            YAxes = new[]
            {
                new Axis
                {
                    Name = "점수",
                    MinLimit = 0,           // 0에서 시작
                    MaxLimit = 5,           // 5에서 고정
                    MinStep  = 1,           // 눈금 간격 1점
                    TextSize = 14,
                    NamePaint   = new SolidColorPaint(SKColors.Black) { SKTypeface = tf },
                    LabelsPaint = new SolidColorPaint(SKColors.Black) { SKTypeface = tf }
                }
            };

            Title = "감정 진단 결과 (평균 점수)";
            OnPropertyChanged(nameof(Title));
        }

        // 2결과 요약 텍스트 갱신 (DeepTestViewModel에서 호출)
        public void SetResultText(string summary)
        {
            ResultText = summary ?? string.Empty;
        }

        //  현실적인 조언 리스트 반영 (DeepTestViewModel에서 호출)
        public void SetRealisticAdvice(JArray adviceArray)
        {
            RealisticAdvice.Clear();
            if (adviceArray == null) return;

            foreach (var t in adviceArray)
            {
                var s = t?.ToString();
                if (!string.IsNullOrWhiteSpace(s))
                    RealisticAdvice.Add(s);
            }
        }
    }
}


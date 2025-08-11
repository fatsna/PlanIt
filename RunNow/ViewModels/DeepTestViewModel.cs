using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using RunNow.Core;
using RunNow.Models;
using RunNow.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RunNow.ViewModels
{
    public partial class DeepTestViewModel : ObservableObject
    {
        private readonly TcpClientService _tcpService;
        private readonly NavigationStore _navigationStore;
        private readonly IServiceProvider _serviceProvider;

        public ObservableCollection<DeepQuestionItem> Questions { get; } = new();
        public ObservableCollection<InterestItem> InterestCategories { get; } = new();

        [ObservableProperty] private bool isEmotionStepVisible = true;
        [ObservableProperty] private bool isFinanceStepVisible;
        [ObservableProperty] private bool isCareerStepVisible;

        // XAML 바인딩용
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(AnalyzeCommand))]
        private string currentPosition = string.Empty;

        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(AnalyzeCommand))]
        private string selectedExperience = string.Empty;

        partial void OnCurrentPositionChanged(string value) => Position = value;
        partial void OnSelectedExperienceChanged(string value) => Experience = value;

        // 재정 입력
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(GoToCareerStepCommand))] private string assets = string.Empty;
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(GoToCareerStepCommand))] private string income = string.Empty;
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(GoToCareerStepCommand))] private string rent = string.Empty;
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(GoToCareerStepCommand))] private string phone = string.Empty;
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(GoToCareerStepCommand))] private string subscription = string.Empty;
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(GoToCareerStepCommand))] private string food = string.Empty;
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(GoToCareerStepCommand))] private string transport = string.Empty;
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(GoToCareerStepCommand))] private string leisure = string.Empty;
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(GoToCareerStepCommand))] private string savingTarget = string.Empty;
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(GoToCareerStepCommand))] private string debt = string.Empty;

        // payload용 경력
        [ObservableProperty] private string position = string.Empty;
        [ObservableProperty] private string experience = string.Empty;
        [ObservableProperty] private string skills = string.Empty; // 선택 항목

        public DeepTestViewModel(TcpClientService tcpService, NavigationStore navigationStore, IServiceProvider serviceProvider)
        {
            _tcpService = tcpService;
            _navigationStore = navigationStore;
            _serviceProvider = serviceProvider;


            var questionsByCategory = new (string Category, string QuestionText)[]
            {
                ("감정 상태", "요즘 일이 잘 풀릴 거라는 희망이 전혀 들지 않는다"),
                ("감정 상태", "기분이 가라앉고 우울한 날이 대부분이다"),
                ("감정 상태", "아무것도 하고 싶지 않고 무기력하다"),
                ("감정 상태", "기뻤던 기억이 거의 없다"),
                ("감정 상태", "스트레스를 받을 때 긍정적으로 생각하지 못한다"),
                ("이직 욕구/동기 상태", "지금의 직장에 더 이상 다니고 싶지 않다"),
                ("이직 욕구/동기 상태", "이직 생각이 머릿속을 떠나지 않는다"),
                ("이직 욕구/동기 상태", "지금보다 더 나은 환경이 따로 있을 것 같다"),
                ("이직 욕구/동기 상태", "현재 회사는 내 노력을 인정해주지 않는다"),
                ("이직 욕구/동기 상태", "이 조직에서 내 미래가 보이지 않는다"),
                ("번아웃/스트레스 지수", "충분히 쉬어도 피곤하고 회복되지 않는다"),
                ("번아웃/스트레스 지수", "출근 생각만 해도 마음이 무겁고 괴롭다"),
                ("번아웃/스트레스 지수", "일하면서 감정이 마비되거나 무감각해진다"),
                ("번아웃/스트레스 지수", "매일이 버티는 것처럼 느껴진다"),
                ("번아웃/스트레스 지수", "사람들과의 대화가 버겁고 혼자 있고 싶다"),
                ("회복탄력성 / 에너지 상태", "힘든 상황에서 쉽게 무너진다"),
                ("회복탄력성 / 에너지 상태", "스트레스를 해소할 방법이 없다"),
                ("회복탄력성 / 에너지 상태", "에너지를 회복할 시간이나 여유가 없다"),
                ("회복탄력성 / 에너지 상태", "힘든 일이 생기면 다시 집중하기 어렵다"),
                ("회복탄력성 / 에너지 상태", "감정적으로 힘들 때 스스로 다독이지 못한다"),
                ("가치/비전 일치도", "회사의 운영방식이나 문화가 나와 전혀 맞지 않는다"),
                ("가치/비전 일치도", "나의 핵심 가치가 회사에서 전혀 존중받지 못한다"),
                ("가치/비전 일치도", "회사의 방향성과 내 인생 목표가 완전히 다르다"),
                ("가치/비전 일치도", "현재의 삶은 내가 바라는 삶과 많이 다르다"),
                ("가치/비전 일치도", "조직보다 나의 성장에만 더 집중하고 싶다"),
                ("관계 스트레스", "상사나 동료와의 관계가 심리적으로 너무 힘들다"),
                ("관계 스트레스", "직장 내에서 심리적 안전감을 전혀 느낄 수 없다"),
                ("관계 스트레스", "내 주변 사람들은 내 고민을 이해하지 못한다"),
                ("관계 스트레스", "나는 팀에서 존중받지 못한다고 느낀다"),
                ("관계 스트레스", "가까운 사람들과도 감정적으로 거리감이 느껴진다"),
                ("자기정체감/불확실성 상태", "내가 정말 원하는 것이 무엇인지 전혀 모르겠다"),
                ("자기정체감/불확실성 상태", "앞으로 어떻게 나아가야 할지 방향이 전혀 보이지 않는다"),
                ("자기정체감/불확실성 상태", "나는 어떤 사람인지 스스로도 잘 모르겠다"),
                ("자기정체감/불확실성 상태", "지금 나는 멈춰있고 제자리걸음 중인 것 같다"),
                ("자기정체감/불확실성 상태", "삶에 대한 주도권을 잃은 느낌이 든다"),
            };

            int index = 1;
            foreach (var (category, question) in questionsByCategory)
            {

                Questions.Add(new DeepQuestionItem { Id = $"Q{index++}", Category = category, QuestionText = question });
            }

            // 그룹 포함 관심산업 채우기
            Add("공공 & 사회", "공공", "교육", "에너지", "환경", "ESG", "정부정책", "사회복지", "국방/안보");
            Add("헬스케어", "의료", "바이오", "헬스케어", "의료기기", "디지털헬스", "제약", "임상시험");
            Add("금융 & 핀테크", "금융", "핀테크", "보험", "블록체인", "투자", "회계/세무", "부동산금융");
            Add("산업 & 제조", "제조", "스마트팩토리", "건설", "모빌리티", "반도체", "기계/설비", "전기/전자");
            Add("콘텐츠 & IT", "게임", "IT/SW", "보안", "클라우드", "AI", "메타버스", "XR/VR/AR", "영상/미디어");
            Add("유통 & 이커머스", "유통", "이커머스", "브랜드", "리테일테크", "라이브커머스");
            Add("물류 & 공급망", "물류", "SCM", "배송", "3PL", "창고관리");
            Add("HR & 조직문화", "HR", "채용", "조직문화", "복지", "교육훈련");
            Add("스타트업 & 혁신", "스타트업", "Social Impact", "GreenTech", "Tech for Good", "임팩트 투자");

            void Add(string group, params string[] names)
            {
                foreach (var n in names)
                    InterestCategories.Add(new InterestItem { Group = group, Name = n });
            }

            // 이벤트 훅
            HookInterestEvents();
        }

        [RelayCommand]
        private void GoToFinanceStep()
        {
            if (Questions.Any(q => !q.SelectedValue.HasValue))
            {
                MessageBox.Show("모든 문항에 응답해주세요.");
                return;
            }

            IsEmotionStepVisible = false;
            IsFinanceStepVisible = true;
        }


        [RelayCommand(CanExecute = nameof(CanGoToCareerStep))]
        private void GoToCareerStep()
        {

            IsFinanceStepVisible = false;
            IsCareerStepVisible = true;
        }


        private bool CanGoToCareerStep()
        {
            bool Has(string s) => !string.IsNullOrWhiteSpace(s);
            return Has(Assets) && Has(Income) && Has(Rent) && Has(Phone) && Has(Subscription)
                && Has(Food) && Has(Transport) && Has(Leisure) && Has(SavingTarget) && Has(Debt);
        }


        private List<JObject> CalculateEmotionResults()
        {
            return Questions
                .GroupBy(q => q.Category)

                .Select(g => new JObject
                {
                    ["category"] = g.Key,
                    ["average"] = g.Average(q => q.SelectedValue ?? 0)
                })
                .ToList();
        }

        private List<EmotionResultItem> ConvertEmotionResultsToItems(List<JObject> emotionResults)
        {
            return emotionResults.Select(r => new EmotionResultItem
            {
                Category = r["category"]?.ToString() ?? "",

                Score = (int)Math.Round(r["average"]?.ToObject<double>() ?? 0),

                Description = $"{r["category"]?.ToString()} 평균 {r["average"]?.ToObject<double>():F1}점"
            }).ToList();
        }

        public ObservableCollection<RecommendedJobItem> RecommendedJobs { get; } = new();

        public void SetRecommendedJobs(JArray jobs)
        {
            RecommendedJobs.Clear();
            foreach (var item in jobs)
            {
                RecommendedJobs.Add(new RecommendedJobItem
                {
                    Job = item["job"]?.ToString(),
                    Description = item["description"]?.ToString(),
                    Reason = item["reason"]?.ToString()
                });
            }
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AnalyzeCommand))]
        private bool isBusy;

        [RelayCommand(CanExecute = nameof(CanAnalyze))]
        private async Task AnalyzeAsync()
        {

            if (Questions.Any(q => !q.SelectedValue.HasValue))
            {
                MessageBox.Show("모든 문항에 응답해주세요.");
                return;
            }


            IsBusy = true;              // ⬅️ 시작: 오버레이 ON
            try
            {
                var emotionResults = CalculateEmotionResults();

                var finance = new JObject
                {
                    ["assets"] = Assets,
                    ["income"] = Income,
                    ["saving_target"] = SavingTarget,
                    ["debt"] = Debt,
                    ["fixed_expense"] = new JObject
                    {
                        ["rent"] = Rent,
                        ["phone"] = Phone,
                        ["subscription"] = Subscription
                    },
                    ["variable_expense"] = new JObject
                    {
                        ["food"] = Food,
                        ["transport"] = Transport,
                        ["leisure"] = Leisure
                    }
                };

                var career = new JObject
                {
                    ["position"] = Position,
                    ["experience"] = Experience,
                    ["skills"] = Skills,
                    ["interests"] = JArray.FromObject(InterestCategories.Where(x => x.IsSelected).Select(x => x.Name))
                };

                var payload = new JObject
                {
                    ["protocol"] = "100_0",
                    ["type"] = "deep_analysis",
                    ["emotion"] = new JArray(emotionResults),
                    ["finance"] = finance,
                    ["career"] = career
                };

                await _tcpService.ConnectAsync();

                var response = await _tcpService.SendJsonToServer(payload);

                var emotionResultItems = ConvertEmotionResultsToItems(emotionResults);

                var deepResultVm = _serviceProvider.GetRequiredService<DeepResultViewModel>();
                deepResultVm.SetResults(emotionResultItems);

                string summary = response?["analysis_summary"]?.ToString() ?? "결과 요약을 불러오지 못했습니다.";
                deepResultVm.SetResultText(summary);

                var recommendedJobs = (JArray?)response?["recommended_jobs"];
                if (recommendedJobs != null) deepResultVm.SetRecommendedJobs(recommendedJobs);

                var realisticAdvice = response?["realistic_advice"] as JArray;
                if (realisticAdvice != null) deepResultVm.SetRealisticAdvice(realisticAdvice);

                _navigationStore.CurrentViewModel = deepResultVm;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"분석 중 오류가 발생했습니다.\n\n{ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;         // ⬅️ 끝: 오버레이 OFF (성공/실패 상관없이)
            }
        }


        private bool CanAnalyze()
        {
            if (IsBusy) return false; // ⬅️ 로딩 중엔 비활성
            if (Questions.Any(q => !q.SelectedValue.HasValue)) return false;

            var hasPosition = !string.IsNullOrWhiteSpace(CurrentPosition);
            var hasExperience = !string.IsNullOrWhiteSpace(SelectedExperience);
            var hasInterests = InterestCategories.Any(i => i.IsSelected);
            return hasPosition && hasExperience && hasInterests;
        }

        private void HookInterestEvents()
        {
            foreach (var it in InterestCategories)
                it.PropertyChanged += OnInterestItemPropertyChanged;

            InterestCategories.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                    foreach (InterestItem it in e.NewItems)
                        it.PropertyChanged += OnInterestItemPropertyChanged;

                if (e.OldItems != null)
                    foreach (InterestItem it in e.OldItems)
                        it.PropertyChanged -= OnInterestItemPropertyChanged;

                AnalyzeCommand?.NotifyCanExecuteChanged();
            };
        }

        private void OnInterestItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InterestItem.IsSelected))
                AnalyzeCommand?.NotifyCanExecuteChanged();
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using RunNow.Core;
using RunNow.Models;
using RunNow.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RunNow.ViewModels
{
    public partial class DeepTestViewModel : ObservableObject
    {
        // TCP 통신 서비스
        private readonly TcpClientService _tcpService;

        // 네비게이션 상태 관리용 스토어
        private readonly NavigationStore _navigationStore;

        // DI 서비스 공급자
        private readonly IServiceProvider _serviceProvider;

        // 질문 목록 (감정 진단 문항)
        public ObservableCollection<DeepQuestionItem> Questions { get; } = new();

        // 관심 산업 분야 목록
        public ObservableCollection<InterestItem> InterestCategories { get; } = new();

        // UI 단계별 표시 제어용 프로퍼티
        [ObservableProperty] private bool isEmotionStepVisible = true;  // 감정 진단 단계 보임 여부
        [ObservableProperty] private bool isFinanceStepVisible;          // 재정 진단 단계 보임 여부
        [ObservableProperty] private bool isCareerStepVisible;           // 경력 진단 단계 보임 여부

        // 재정 진단 입력값 프로퍼티
        [ObservableProperty] private string assets = string.Empty;       // 총 자산
        [ObservableProperty] private string income = string.Empty;       // 월 수입
        [ObservableProperty] private string rent = string.Empty;         // 월세 등 고정 지출
        [ObservableProperty] private string phone = string.Empty;        // 통신비
        [ObservableProperty] private string subscription = string.Empty; // 구독료
        [ObservableProperty] private string food = string.Empty;         // 식비
        [ObservableProperty] private string transport = string.Empty;    // 교통비
        [ObservableProperty] private string leisure = string.Empty;      // 여가비용
        [ObservableProperty] private string savingTarget = string.Empty; // 저축 목표
        [ObservableProperty] private string debt = string.Empty;         // 부채

        // 경력 진단 입력값 프로퍼티
        [ObservableProperty] private string position = string.Empty;     // 현재 직무
        [ObservableProperty] private string experience = string.Empty;   // 경력 연수
        [ObservableProperty] private string skills = string.Empty;       // 기술/자격증

        // 생성자: DI 주입 및 초기 질문, 관심분야 리스트 구성
        public DeepTestViewModel(
            TcpClientService tcpService,
            NavigationStore navigationStore,
            IServiceProvider serviceProvider)
        {
            _tcpService = tcpService;
            _navigationStore = navigationStore;
            _serviceProvider = serviceProvider;

            // 카테고리와 문항 묶어서 초기화
            var questionsByCategory = new (string Category, string QuestionText)[]
            {
            // 감정 상태
            ("감정 상태", "요즘 일이 잘 풀릴 거라는 희망이 전혀 들지 않는다"),
            ("감정 상태", "기분이 가라앉고 우울한 날이 대부분이다"),
            ("감정 상태", "아무것도 하고 싶지 않고 무기력하다"),
            ("감정 상태", "기뻤던 기억이 거의 없다"),
            ("감정 상태", "스트레스를 받을 때 긍정적으로 생각하지 못한다"),

            // 이직 욕구/동기 상태
            ("이직 욕구/동기 상태", "지금의 직장에 더 이상 다니고 싶지 않다"),
            ("이직 욕구/동기 상태", "이직 생각이 머릿속을 떠나지 않는다"),
            ("이직 욕구/동기 상태", "지금보다 더 나은 환경이 따로 있을 것 같다"),
            ("이직 욕구/동기 상태", "현재 회사는 내 노력을 인정해주지 않는다"),
            ("이직 욕구/동기 상태", "이 조직에서 내 미래가 보이지 않는다"),

            // 번아웃/스트레스 지수
            ("번아웃/스트레스 지수", "충분히 쉬어도 피곤하고 회복되지 않는다"),
            ("번아웃/스트레스 지수", "출근 생각만 해도 마음이 무겁고 괴롭다"),
            ("번아웃/스트레스 지수", "일하면서 감정이 마비되거나 무감각해진다"),
            ("번아웃/스트레스 지수", "매일이 버티는 것처럼 느껴진다"),
            ("번아웃/스트레스 지수", "사람들과의 대화가 버겁고 혼자 있고 싶다"),

            // 회복탄력성 / 에너지 상태
            ("회복탄력성 / 에너지 상태", "힘든 상황에서 쉽게 무너진다"),
            ("회복탄력성 / 에너지 상태", "스트레스를 해소할 방법이 없다"),
            ("회복탄력성 / 에너지 상태", "에너지를 회복할 시간이나 여유가 없다"),
            ("회복탄력성 / 에너지 상태", "힘든 일이 생기면 다시 집중하기 어렵다"),
            ("회복탄력성 / 에너지 상태", "감정적으로 힘들 때 스스로 다독이지 못한다"),

            // 가치/비전 일치도
            ("가치/비전 일치도", "회사의 운영방식이나 문화가 나와 전혀 맞지 않는다"),
            ("가치/비전 일치도", "나의 핵심 가치가 회사에서 전혀 존중받지 못한다"),
            ("가치/비전 일치도", "회사의 방향성과 내 인생 목표가 완전히 다르다"),
            ("가치/비전 일치도", "현재의 삶은 내가 바라는 삶과 많이 다르다"),
            ("가치/비전 일치도", "조직보다 나의 성장에만 더 집중하고 싶다"),

            // 관계 스트레스
            ("관계 스트레스", "상사나 동료와의 관계가 심리적으로 너무 힘들다"),
            ("관계 스트레스", "직장 내에서 심리적 안전감을 전혀 느낄 수 없다"),
            ("관계 스트레스", "내 주변 사람들은 내 고민을 이해하지 못한다"),
            ("관계 스트레스", "나는 팀에서 존중받지 못한다고 느낀다"),
            ("관계 스트레스", "가까운 사람들과도 감정적으로 거리감이 느껴진다"),

            // 자기정체감/불확실성 상태
            ("자기정체감/불확실성 상태", "내가 정말 원하는 것이 무엇인지 전혀 모르겠다"),
            ("자기정체감/불확실성 상태", "앞으로 어떻게 나아가야 할지 방향이 전혀 보이지 않는다"),
            ("자기정체감/불확실성 상태", "나는 어떤 사람인지 스스로도 잘 모르겠다"),
            ("자기정체감/불확실성 상태", "지금 나는 멈춰있고 제자리걸음 중인 것 같다"),
            ("자기정체감/불확실성 상태", "삶에 대한 주도권을 잃은 느낌이 든다"),
            };

            int index = 1;
            foreach (var (category, question) in questionsByCategory)
            {
                Questions.Add(new DeepQuestionItem
                {
                    Id = $"Q{index++}",
                    Category = category,
                    QuestionText = question
                });
            }

            // 관심 산업 초기화는 그대로 유지
            InterestCategories.Add(new InterestItem { Name = "공공" });
            InterestCategories.Add(new InterestItem { Name = "교육" });
            InterestCategories.Add(new InterestItem { Name = "에너지" });
            InterestCategories.Add(new InterestItem { Name = "헬스케어" });
            InterestCategories.Add(new InterestItem { Name = "AI" });
            InterestCategories.Add(new InterestItem { Name = "게임" });
        }


        // 감정 단계에서 재정 단계로 넘어갈 때 호출하는 커맨드
        [RelayCommand]
        private void GoToFinanceStep()
        {
            // 응답 누락 문항 체크
            if (Questions.Any(q => !q.SelectedValue.HasValue))
            {
                MessageBox.Show("모든 문항에 응답해주세요.");
                return;
            }
            // UI 표시 상태 전환
            IsEmotionStepVisible = false;
            IsFinanceStepVisible = true;
        }

        // 재정 단계에서 경력 단계로 넘어갈 때 호출하는 커맨드
        [RelayCommand]
        private void GoToCareerStep()
        {
            // UI 표시 상태 전환
            IsFinanceStepVisible = false;
            IsCareerStepVisible = true;
        }

        // 감정 진단 결과 계산 - 각 카테고리별 평균 점수 계산하여 리스트 반환
        private List<JObject> CalculateEmotionResults()
        {
            return Questions
                .GroupBy(q => q.Category)
                .Select(g =>
                {
                    var avg = g.Average(q => q.SelectedValue ?? 0);
                    return new JObject
                    {
                        ["category"] = g.Key,
                        ["average"] = avg
                    };
                })
                .ToList();
        }
        // 감정 결과를 UI용 아이템 리스트로 변환
        private List<EmotionResultItem> ConvertEmotionResultsToItems(List<JObject> emotionResults)
        {
            return emotionResults.Select(r => new EmotionResultItem
            {
                Category = r["category"]?.ToString() ?? "",
                Score = (int)System.Math.Round(r["average"]?.ToObject<double>() ?? 0),
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

        // 분석 실행 커맨드 (비동기)
        [RelayCommand]
        private async Task AnalyzeAsync()
        {
            // 필수 문항 응답 체크
            if (Questions.Any(q => !q.SelectedValue.HasValue))
            {
                MessageBox.Show("모든 문항에 응답해주세요.");
                return;
            }

            // 감정 진단 결과 계산
            var emotionResults = CalculateEmotionResults();

            // 재정 데이터 JSON 생성
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

            // 경력 데이터 JSON 생성
            var career = new JObject
            {
                ["position"] = Position,
                ["experience"] = Experience,
                ["skills"] = Skills,
                ["interests"] = JArray.FromObject(InterestCategories.Where(x => x.IsSelected).Select(x => x.Name))
            };

            // 최종 전송 payload 생성
            var payload = new JObject
            {
                ["protocol"] = "100_0",
                ["type"] = "deep_analysis",
                ["emotion"] = new JArray(emotionResults),
                ["finance"] = finance,
                ["career"] = career
            };

            // TCP 서버 연결 및 데이터 전송
            await _tcpService.ConnectAsync();

            // 서버한테 어떻게 쏘는지
            Console.WriteLine("최종 payload:");
            Console.WriteLine(payload.ToString());

            var response = await _tcpService.SendJsonToServer(payload);
            //디버기용ㅇ
            Console.WriteLine("서버 응답:");
            Console.WriteLine(response?.ToString());

            // 감정 결과를 UI 표시용 아이템으로 변환
            var emotionResultItems = ConvertEmotionResultsToItems(emotionResults);

            // 결과 페이지 ViewModel 생성 및 결과 세팅
            var deepResultVm = _serviceProvider.GetRequiredService<DeepResultViewModel>();
            deepResultVm.SetResults(emotionResultItems);

            // 서버 응답에서 summary 항목 추출 후 텍스트 결과 설정
            string summary = response?["analysis_summary"]?.ToString() ?? "결과 요약을 불러오지 못했습니다.";
            deepResultVm.SetResultText(summary);

            var recommendedJobs = (JArray?)response?["recommended_jobs"];
            if (recommendedJobs != null)
            {
                deepResultVm.SetRecommendedJobs(recommendedJobs);
            }

            // 화면 전환 (네비게이션)
            _navigationStore.CurrentViewModel = deepResultVm;

            // 서버 응답 메시지 출력
            MessageBox.Show($"서버 응답:\n{response}", "분석 결과", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

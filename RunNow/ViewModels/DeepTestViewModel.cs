using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using RunNow.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using RunNow.Models;

namespace RunNow.ViewModels
{
    public partial class DeepTestViewModel : ObservableObject
    {
        private readonly TcpClientService _tcpService;

        public ObservableCollection<DeepQuestionItem> Questions { get; } = new();
        public ObservableCollection<InterestItem> InterestCategories { get; } = new();

        [ObservableProperty] private bool isEmotionStepVisible = true;
        [ObservableProperty] private bool isFinanceStepVisible;
        [ObservableProperty] private bool isCareerStepVisible;

        [ObservableProperty] private string assets = string.Empty;
        [ObservableProperty] private string income = string.Empty;
        [ObservableProperty] private string rent = string.Empty;
        [ObservableProperty] private string phone = string.Empty;
        [ObservableProperty] private string subscription = string.Empty;
        [ObservableProperty] private string food = string.Empty;
        [ObservableProperty] private string transport = string.Empty;
        [ObservableProperty] private string leisure = string.Empty;
        [ObservableProperty] private string savingTarget = string.Empty;
        [ObservableProperty] private string debt = string.Empty;

        [ObservableProperty] private string position = string.Empty;
        [ObservableProperty] private string experience = string.Empty;
        [ObservableProperty] private string skills = string.Empty;

        public DeepTestViewModel(TcpClientService tcpService)
        {
            _tcpService = tcpService;

            // 30개 감정 설문
            var texts = new[]
            {
                "일이 잘 풀릴 거란 희망이 잘 들지 않는다",
                "요즘 기분이 가라앉고 우울한 날이 많다",
                "아무것도 하고 싶지 않은 무기력감이 든다",
                "최근 웃거나 기뻤던 기억이 있다",
                "스트레스 상황에서도 긍정적으로 생각하려 한다",
                "지금의 직장을 계속 다닐 생각이 없다",
                "이직에 대한 생각이 머릿속을 자주 맴돈다",
                "내가 더 잘할 수 있는 환경이 따로 있다고 느낀다",
                "현재 회사에서 내 노력을 제대로 인정받지 못한다",
                "현재 조직에서 나의 미래가 그려지지 않는다",
                "아무리 쉬어도 피곤하고 회복이 되지 않는다",
                "출근 생각만 해도 마음이 무겁다",
                "일하는 도중 감정이 마비되거나 무감각해진다",
                "매일매일이 버티는 것처럼 느껴진다",
                "사람들과 대화할 힘도 없고, 혼자 있고 싶다",
                "힘든 상황에서도 다시 일어설 수 있다는 믿음이 있다",
                "스트레스를 적절히 해소하는 나만의 방식이 있다",
                "내 에너지를 다시 채울 수 있는 시간이 충분하다",
                "일이 힘들어도 나는 다시 집중할 수 있는 편이다",
                "감정적으로 무너질 것 같을 때 스스로를 다독일 수 있다",
                "지금 회사의 운영방식이나 문화가 나와 맞지 않는다",
                "나의 핵심가치가 존중받지 않는다",
                "회사가 추구하는 방향성과 내 인생 목표가 다르다",
                "나는 내가 바라는 삶과 지금의 삶이 일치한다고 느낀다",
                "나는 조직보다는 개인 성장에 더 가치를 둔다",
                "상사나 동료와의 관계가 심리적으로 힘들다",
                "직장 내에서 심리적 안전감을 느끼기 어렵다",
                "주변 사람들이 내 고민을 잘 이해해주지 않는다",
                "나는 내가 정말 원하는 게 뭔지 잘 모르겠다",
                "지금 나는 멈춰 있고, 어디로 가야 할지 모르겠다"
            };

            int i = 1;
            foreach (var text in texts)
            {
                Questions.Add(new DeepQuestionItem { Id = $"Q{i++}", QuestionText = text });
            }

            // 관심 산업 카테고리
            InterestCategories.Add(new InterestItem { Name = "공공" });
            InterestCategories.Add(new InterestItem { Name = "교육" });
            InterestCategories.Add(new InterestItem { Name = "에너지" });
            InterestCategories.Add(new InterestItem { Name = "헬스케어" });
            InterestCategories.Add(new InterestItem { Name = "AI" });
            InterestCategories.Add(new InterestItem { Name = "게임" });
            // 필요시 나머지도 추가
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

        [RelayCommand]
        private void GoToCareerStep()
        {
            IsFinanceStepVisible = false;
            IsCareerStepVisible = true;
        }

        [RelayCommand]
        private async Task AnalyzeAsync()
        {
            var payload = new JObject
            {
                ["type"] = "deep_analysis",
                ["emotion"] = JArray.FromObject(Questions),
                ["finance"] = new JObject
                {
                    ["assets"] = Assets,
                    ["income"] = Income,
                    ["rent"] = Rent,
                    ["phone"] = Phone,
                    ["subscription"] = Subscription,
                    ["food"] = Food,
                    ["transport"] = Transport,
                    ["leisure"] = Leisure,
                    ["savingTarget"] = SavingTarget,
                    ["debt"] = Debt
                },
                ["career"] = new JObject
                {
                    ["position"] = Position,
                    ["experience"] = Experience,
                    ["skills"] = Skills,
                    ["interests"] = JArray.FromObject(InterestCategories.Where(x => x.IsSelected).Select(x => x.Name))
                }
            };

            await _tcpService.ConnectAsync();
            var response = await _tcpService.SendJsonToServer(payload);

            MessageBox.Show($"서버 응답:\n{response}", "분석 결과", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

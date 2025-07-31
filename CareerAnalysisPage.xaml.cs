using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ByeCompany;


namespace ByeCompany
{
    public partial class CareerAnalysisPage : Page
    {
        // 직무 키워드 기반 산업군 추천
        private readonly Dictionary<string, List<string>> PositionToIndustries = new()
{
    { "개발", new List<string> { "핀테크", "헬스케어", "공공 IT", "플랫폼" } },
    { "디자인", new List<string> { "게임", "광고", "모바일 앱" } },
    { "회계", new List<string> { "금융", "공공", "세무" } },
    { "마케팅", new List<string> { "유통", "이커머스", "스타트업" } }
};

        // 보완 기술 추천
        private readonly Dictionary<string, List<string>> SkillToReinforce = new()
{
    { "Java", new List<string> { "Spring Boot", "AWS" } },
    { "C#", new List<string> { ".NET Core", "Azure" } },
    { "SQL", new List<string> { "DB 튜닝", "NoSQL" } }
};

        // 유사 사례 샘플
        private readonly List<string> SampleCases = new()
{
    "🧑‍💻 3년차 Java 개발자 → 공공 SI → 대기업 이직",
    "🎨 UI 디자이너 → 게임 회사 → UX 전문 에이전시 전직",
    "📊 회계 경력 5년 → 공공기관 → 스타트업 CFO 보조 역할"
};

        public CareerAnalysisPage()
        {
            InitializeComponent();
        }

        private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            var profile = new CareerProfile
            {
                CurrentPosition = PositionTextBox.Text,
                YearsOfExperience = ExperienceComboBox.SelectedIndex switch
                {
                    0 => 0,
                    1 => 2,
                    2 => 4,
                    3 => 7,
                    4 => 11,
                    _ => 0
                },
                SkillsAndCerts = SkillsTextBox.Text.Split(',').Select(s => s.Trim()).ToList(),
                InterestedIndustries = GetCheckedIndustries()
            };

            ShowCareerAnalysis(profile);
        }

        private List<string> GetCheckedIndustries()
        {
            return InterestPanel.Children.OfType<CheckBox>()
                .Where(cb => cb.IsChecked == true)
                .Select(cb => cb.Tag?.ToString() ?? "")
                .ToList();
        }

        private void ShowCareerAnalysis(CareerProfile profile)
        {
            // 🔎 직무 키워드에 따라 산업군 추천
            var matchedIndustries = PositionToIndustries
                .Where(kv => profile.CurrentPosition.Contains(kv.Key))
                .SelectMany(kv => kv.Value)
                .Distinct()
                .ToList();

            // 📚 현재 기술 기반 보완 기술 추천
            var reinforceSkills = new List<string>();
            foreach (var skill in profile.SkillsAndCerts)
            {
                if (SkillToReinforce.TryGetValue(skill, out var recs))
                    reinforceSkills.AddRange(recs);
            }

            // 🧭 유사 이직 사례 중 하나 랜덤 출력
            var random = new Random();
            var exampleCase = SampleCases[random.Next(SampleCases.Count)];

            // 💬 메시지 출력
            string message =
                $"✅ [요약 분석]\n" +
                $"직무: {profile.CurrentPosition}\n" +
                $"경력: {profile.YearsOfExperience}년\n" +
                $"기술: {string.Join(", ", profile.SkillsAndCerts)}\n" +
                $"관심 산업: {string.Join(", ", profile.InterestedIndustries)}\n\n" +

                $"🔎 [추천 산업군]\n{(matchedIndustries.Count > 0 ? string.Join(", ", matchedIndustries) : "해당 없음")}\n\n" +
                $"📚 [보완 기술 추천]\n{(reinforceSkills.Count > 0 ? string.Join(", ", reinforceSkills.Distinct()) : "없음")}\n\n" +
                $"🧭 [유사 이직 사례]\n{exampleCase}";

            MessageBox.Show(message, "경력 분석 결과", MessageBoxButton.OK, MessageBoxImage.Information);
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ByeCompany
{
    public partial class CareerAnalysisPage : Page
    {
        // 직무 키워드 기반 산업군 추천
        private readonly Dictionary<string, List<string>> PositionToIndustries = new()
        {
            { "개발", new List<string> { "핀테크", "헬스케어", "공공 IT", "플랫폼", "보안", "이커머스", "스마트팩토리" } },
            { "디자인", new List<string> { "게임", "엔터테인먼트", "모바일 앱", "UX/UI", "광고", "브랜딩" } },
            { "회계", new List<string> { "금융", "공공", "세무", "스타트업 재무", "ERP" } },
            { "마케팅", new List<string> { "유통", "이커머스", "스타트업", "브랜드 마케팅", "콘텐츠 마케팅" } },
            { "HR", new List<string> { "HR SaaS", "채용 플랫폼", "사내 교육", "조직문화", "복지 서비스" } },
            { "물류", new List<string> { "3PL", "스마트물류", "유통", "배송 플랫폼" } }
        };

                // 보완 기술 추천
                private readonly Dictionary<string, List<string>> SkillToReinforce = new()
        {
            { "Java", new List<string> { "Spring Boot", "JPA", "AWS", "Kafka" } },
            { "C#", new List<string> { ".NET Core", "Azure", "WPF", "Blazor" } },
            { "Python", new List<string> { "Pandas", "Django", "Flask", "ML/DL", "FastAPI" } },
            { "JavaScript", new List<string> { "React", "Vue.js", "TypeScript", "Next.js" } },
            { "SQL", new List<string> { "DB 튜닝", "NoSQL", "MongoDB", "Redis" } },
            { "Figma", new List<string> { "UX 리서치", "Design System", "프로토타이핑" } }
        };

                // 유사 사례 샘플 (분야별 다양화)
                private readonly List<string> SampleCases = new()
        {
            // 🧑‍💻 개발 분야
            "🧑‍💻 3년차 Java 개발자 → 공공 SI → 대기업 이직",
            "🧑‍💻 프론트엔드 개발자 → 핀테크 스타트업 → 테크 리드 성장",
            "🧑‍💻 C# 개발자 → 제조업 MES 시스템 → WPF 전문가로 전환",

            // 🎨 디자인
            "🎨 UI 디자이너 → 게임 회사 → UX 전문 에이전시 전직",
            "🎨 브랜드 디자이너 → 이커머스 마케팅팀 → 콘텐츠 디렉터로 전환",

            // 📊 회계/재무
            "📊 회계 경력 5년 → 공공기관 → 스타트업 CFO 보조 역할",
            "📊 세무사 → 회계법인 → 핀테크 자문사 전직",

            // 📣 마케팅
            "📣 콘텐츠 마케터 → 스타트업 → 브랜드 매니저로 전환",
            "📣 퍼포먼스 마케터 → 광고 대행사 → 이커머스 인하우스 전직",

            // 🧑‍💼 HR
            "🧑‍💼 인사 담당자 → HR SaaS 기획자 → B2B PM 전환",
            "🧑‍💼 리크루터 → 채용 플랫폼 운영팀 → 서비스 기획으로 확장",

            // 🚚 물류/유통
            "🚚 배송 기사 출신 → 물류 플랫폼 운영 매니저 이직",
            "🚚 SCM 담당자 → 스마트 물류 자동화 시스템 분석가 전환",

            // 🏛 공공 & 기타
            "🏛 교육 공무원 → 에듀테크 기업 교육 운영자로 이직",
            "🏛 환경 전공자 → ESG 컨설팅 회사 → 전략팀 성장"
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
                SkillsAndCerts = SkillsTextBox.Text.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList(),
                InterestedIndustries = GetCheckedIndustries()
            };

            ShowCareerAnalysis(profile);
        }

        private List<string> GetCheckedIndustries()
        {
            var selectedIndustries = new List<string>();

            foreach (var group in InterestPanel.Children)
            {
                if (group is WrapPanel wp)
                {
                    foreach (var cb in wp.Children.OfType<CheckBox>())
                    {
                        if (cb.IsChecked == true && cb.Tag != null)
                        {
                            selectedIndustries.Add(cb.Tag.ToString()!);
                        }
                    }
                }
            }

            return selectedIndustries;
        }


        private void ShowCareerAnalysis(CareerProfile profile)
        {
            // 직무 키워드에 따라 산업군 추천
            var matchedIndustries = PositionToIndustries
                .Where(kv => profile.CurrentPosition.Contains(kv.Key))
                .SelectMany(kv => kv.Value)
                .Distinct()
                .ToList();

            // 보완 기술 추천
            var reinforceSkills = new List<string>();
            foreach (var skill in profile.SkillsAndCerts)
            {
                if (SkillToReinforce.TryGetValue(skill, out var recs))
                {
                    reinforceSkills.AddRange(recs);
                }
            }

            // 유사 이직 사례 랜덤 출력
            var random = new Random();
            var exampleCase = SampleCases[random.Next(SampleCases.Count)];

            // 분석 결과 메시지 구성
            string message =
                $"✅ [요약 분석]\n" +
                $"직무: {profile.CurrentPosition}\n" +
                $"경력: {profile.YearsOfExperience}년\n" +
                $"기술: {string.Join(", ", profile.SkillsAndCerts)}\n" +
                $"관심 산업: {string.Join(", ", profile.InterestedIndustries)}\n\n" +

                $"🔎 [추천 산업군]\n{(matchedIndustries.Any() ? string.Join(", ", matchedIndustries) : "해당 없음")}\n\n" +
                $"📚 [보완 기술 추천]\n{(reinforceSkills.Any() ? string.Join(", ", reinforceSkills.Distinct()) : "없음")}\n\n" +
                $"🧭 [유사 이직 사례]\n{exampleCase}";

            MessageBox.Show(message, "경력 분석 결과", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RunNow.Services;

namespace RunNow.ViewModels
{
    public partial class CareerAnalysisViewModel : ObservableObject
    {
        [ObservableProperty] private string currentPosition;
        [ObservableProperty] private string experience;
        [ObservableProperty] private string skills;
        [ObservableProperty] private string analysisResult;

        public ObservableCollection<InterestCategory> InterestCategories { get; } = new();

        private readonly Dictionary<string, List<string>> PositionToIndustries = new()
        {
            { "개발", new() { "핀테크", "헬스케어", "공공 IT", "플랫폼", "보안", "이커머스", "스마트팩토리" } },
            { "디자인", new() { "게임", "엔터테인먼트", "모바일 앱", "UX/UI", "광고", "브랜딩" } },
            { "회계", new() { "금융", "공공", "세무", "스타트업 재무", "ERP" } },
            { "마케팅", new() { "유통", "이커머스", "스타트업", "브랜드 마케팅", "콘텐츠 마케팅" } },
            { "HR", new() { "HR SaaS", "채용 플랫폼", "사내 교육", "조직문화", "복지 서비스" } },
            { "물류", new() { "3PL", "스마트물류", "유통", "배송 플랫폼" } }
        };

        private readonly Dictionary<string, List<string>> SkillToReinforce = new()
        {
            { "Java", new() { "Spring Boot", "JPA", "AWS", "Kafka" } },
            { "C#", new() { ".NET Core", "Azure", "WPF", "Blazor" } },
            { "Python", new() { "Pandas", "Django", "Flask", "ML/DL", "FastAPI" } },
            { "JavaScript", new() { "React", "Vue.js", "TypeScript", "Next.js" } },
            { "SQL", new() { "DB 튜닝", "NoSQL", "MongoDB", "Redis" } },
            { "Figma", new() { "UX 리서치", "Design System", "프로토타이핑" } }
        };

        private readonly List<string> SampleCases = new()
        {
            "🧑‍💻 3년차 Java 개발자 → 공공 SI → 대기업 이직",
            "🧑‍💻 프론트엔드 개발자 → 핀테크 스타트업 → 테크 리드 성장",
            "🧑‍💻 C# 개발자 → 제조업 MES 시스템 → WPF 전문가로 전환",
            "🎨 UI 디자이너 → 게임 회사 → UX 전문 에이전시 전직",
            "📊 회계 경력 5년 → 공공기관 → 스타트업 CFO 보조 역할"

        };
        private readonly NavigationStore _navigationStore;
        private readonly IDialogService _dialogService;

        public CareerAnalysisViewModel(NavigationStore navigationStore ,IDialogService dialogService)

        {
            _dialogService = dialogService;
            _navigationStore = navigationStore;
            LoadInterestCategories();
        }

        private void LoadInterestCategories()
        {
            InterestCategories.Add(new InterestCategory("공공 & 사회", new[] { "공공", "교육", "에너지", "환경", "ESG" }));
            InterestCategories.Add(new InterestCategory("헬스케어", new[] { "의료", "바이오", "헬스케어", "의료기기", "디지털헬스" }));
            InterestCategories.Add(new InterestCategory("금융 & 핀테크", new[] { "금융", "핀테크", "보험", "블록체인", "투자" }));
            InterestCategories.Add(new InterestCategory("산업 & 제조", new[] { "제조", "스마트팩토리", "건설", "모빌리티", "반도체" }));
            InterestCategories.Add(new InterestCategory("콘텐츠 & IT", new[] { "게임", "IT/SW", "보안", "클라우드", "AI" }));
            InterestCategories.Add(new InterestCategory("유통 & 이커머스", new[] { "유통", "이커머스", "브랜드", "리테일테크" }));
            InterestCategories.Add(new InterestCategory("물류 & 공급망", new[] { "물류", "SCM", "배송", "3PL" }));
            InterestCategories.Add(new InterestCategory("HR & 조직문화", new[] { "HR", "채용", "조직문화", "복지" }));
            InterestCategories.Add(new InterestCategory("스타트업 & 혁신", new[] { "스타트업", "Social Impact", "GreenTech" }));
        }

        [RelayCommand]
        private void Analyze()
        {
            var selectedIndustries = InterestCategories
                .SelectMany(c => c.Options.Where(o => o.IsSelected).Select(o => o.Name))
                .ToList();

            int years = Experience switch
            {
                "1년 미만" => 0,
                "1~3년" => 2,
                "3~5년" => 4,
                "5~10년" => 7,
                "10년 이상" => 11,
                _ => 0
            };

            var matchedIndustries = PositionToIndustries
                .Where(kv => !string.IsNullOrWhiteSpace(CurrentPosition) && CurrentPosition.Contains(kv.Key))
                .SelectMany(kv => kv.Value)
                .Distinct()
                .ToList();

            var reinforceSkills = (Skills ?? string.Empty).Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .SelectMany(s => SkillToReinforce.ContainsKey(s) ? SkillToReinforce[s] : new List<string>())
                .Distinct()
                .ToList();

            var random = new Random();
            var exampleCase = SampleCases[random.Next(SampleCases.Count)];

            string result =
                $"✅ [요약 분석]\n" +
                $"직무: {CurrentPosition}\n" +
                $"경력: {years}년\n" +
                $"기술: {Skills}\n" +
                $"관심 산업: {string.Join(", ", selectedIndustries)}\n\n" +
                $"🔎 [추천 산업군]\n{(matchedIndustries.Any() ? string.Join(", ", matchedIndustries) : "해당 없음")}\n\n" +
                $"📚 [보완 기술 추천]\n{(reinforceSkills.Any() ? string.Join(", ", reinforceSkills) : "없음")}\n\n" +
                $"🧭 [유사 이직 사례]\n{exampleCase}";

            _dialogService.ShowMessage(result, "경력 분석 결과");
        }
    }

    public class InterestCategory
    {
        public string Category { get; }
        public ObservableCollection<InterestOption> Options { get; }

        public InterestCategory(string category, IEnumerable<string> options)
        {
            Category = category;
            Options = new ObservableCollection<InterestOption>(options.Select(o => new InterestOption(o)));
        }
    }

    public partial class InterestOption : ObservableObject
    {
        public string Name { get; }
        [ObservableProperty] private bool isSelected;

        public InterestOption(string name)
        {
            Name = name;
        }
    }
}

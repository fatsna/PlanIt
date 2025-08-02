using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunNow.Core
{
    class Constants
    {
        // 감정분석에 쓰일 질문 구조체
        public class QuestionItem
        {
            public string Category { get; set; }
            public string QuestionText { get; set; }
            public bool IsPositive { get; set; }
        };
        // 감정분석에 쓰일 질문 목록
        public static readonly List<QuestionItem> Questions = new List<QuestionItem>
        {
            new QuestionItem { Category = "감정 상태", QuestionText = "일이 잘 풀릴 거란 희망이 잘 들지 않는다", IsPositive = false },
            new QuestionItem { Category = "감정 상태", QuestionText = "요즘 기분이 가라앉고 우울한 날이 많다", IsPositive = false },
            new QuestionItem { Category = "감정 상태", QuestionText = "평소보다 짜증이나 분노가 많아진 것 같다", IsPositive = false },
            new QuestionItem { Category = "감정 상태", QuestionText = "아무것도 하고 싶지 않은 무기력감이 든다", IsPositive = false },
            new QuestionItem { Category = "감정 상태", QuestionText = "작은 일에도 자주 불안해진다", IsPositive = false },
            new QuestionItem { Category = "감정 상태", QuestionText = "하루가 즐겁거나 의미 있게 느껴질 때가 있다", IsPositive = true },
            new QuestionItem { Category = "감정 상태", QuestionText = "최근 웃거나 기뻤던 기억이 있다", IsPositive = true },
            new QuestionItem { Category = "감정 상태", QuestionText = "나는 내 감정을 잘 조절하고 있는 편이다", IsPositive = true },
            new QuestionItem { Category = "감정 상태", QuestionText = "내 상태를 나 스스로 잘 알고 있다고 느낀다", IsPositive = true },
            new QuestionItem { Category = "감정 상태", QuestionText = "스트레스 상황에서도 긍정적으로 생각하려 한다", IsPositive = true },
            new QuestionItem { Category = "이직 욕구/동기 상태", QuestionText = "지금의 직장을 계속 다닐 생각이 없다", IsPositive = false },
            new QuestionItem { Category = "이직 욕구/동기 상태", QuestionText = "이직에 대한 생각이 머릿속을 자주 맴돈다", IsPositive = false },
            new QuestionItem { Category = "이직 욕구/동기 상태", QuestionText = "새로운 직장을 알아보거나 지원한 적이 있다", IsPositive = false },
            new QuestionItem { Category = "이직 욕구/동기 상태", QuestionText = "내가 더 잘할 수 있는 환경이 따로 있다고 느낀다", IsPositive = false },
            new QuestionItem { Category = "이직 욕구/동기 상태", QuestionText = "나의 성장 가능성이 지금 회사에선 낮아 보인다", IsPositive = false },
            new QuestionItem { Category = "이직 욕구/동기 상태", QuestionText = "현재 일에 대한 몰입도나 열정이 낮다", IsPositive = false },
            new QuestionItem { Category = "이직 욕구/동기 상태", QuestionText = "현재 회사에서 내 노력을 제대로 인정받지 못한다", IsPositive = false },
            new QuestionItem { Category = "이직 욕구/동기 상태", QuestionText = "현재 조직에서 나의 미래가 그려지지 않는다", IsPositive = false },
            new QuestionItem { Category = "번아웃/스트레스 지수", QuestionText = "아무리 쉬어도 피곤하고 회복이 되지 않는다", IsPositive = false },
            new QuestionItem { Category = "번아웃/스트레스 지수", QuestionText = "출근 생각만 해도 마음이 무겁다", IsPositive = false },
            new QuestionItem { Category = "번아웃/스트레스 지수", QuestionText = "일하는 도중 감정이 마비되거나 무감각해진다", IsPositive = false },
            new QuestionItem { Category = "번아웃/스트레스 지수", QuestionText = "사람들과 대화할 힘도 없고, 혼자 있고 싶다", IsPositive = false },
            new QuestionItem { Category = "번아웃/스트레스 지수", QuestionText = "일이 아닌 일상에도 쉽게 짜증이 난다", IsPositive = false },
            new QuestionItem { Category = "번아웃/스트레스 지수", QuestionText = "매일매일이 버티는 것처럼 느껴진다", IsPositive = false },
            new QuestionItem { Category = "회복탄력성 / 에너지 상태", QuestionText = "힘든 상황에서도 다시 일어설 수 있다는 믿음이 있다", IsPositive = true },
            new QuestionItem { Category = "회복탄력성 / 에너지 상태", QuestionText = "스트레스를 적절히 해소하는 나만의 방식이 있다", IsPositive = true },
            new QuestionItem { Category = "회복탄력성 / 에너지 상태", QuestionText = "내 에너지를 다시 채울 수 있는 시간이 충분하다", IsPositive = true },
            new QuestionItem { Category = "회복탄력성 / 에너지 상태", QuestionText = "감정적으로 무너질 것 같을 때 스스로를 다독일 수 있다", IsPositive = true },
            new QuestionItem { Category = "회복탄력성 / 에너지 상태", QuestionText = "일이 힘들어도 나는 다시 집중할 수 있는 편이다", IsPositive = true },
            new QuestionItem { Category = "가치/비전 일치도", QuestionText = "지금 회사의 운영방식이나 문화가 나와 맞지 않는다", IsPositive = false },
            new QuestionItem { Category = "가치/비전 일치도", QuestionText = "나는 조직보다는 개인 성장에 더 가치를 둔다", IsPositive = false },
            new QuestionItem { Category = "가치/비전 일치도", QuestionText = "나의 핵심가치(예: 자율성, 성취감 등)가 존중받지 않는다", IsPositive = false },
            new QuestionItem { Category = "가치/비전 일치도", QuestionText = "회사가 추구하는 방향성과 내 인생 목표가 다르다", IsPositive = false },
            new QuestionItem { Category = "가치/비전 일치도", QuestionText = "나는 내가 바라는 삶과 지금의 삶이 일치한다고 느낀다", IsPositive = true },
            new QuestionItem { Category = "관계 스트레스", QuestionText = "상사나 동료와의 관계가 심리적으로 힘들다", IsPositive = false },
            new QuestionItem { Category = "관계 스트레스", QuestionText = "직장 내에서 심리적 안전감을 느끼기 어렵다", IsPositive = false },
            new QuestionItem { Category = "관계 스트레스", QuestionText = "내가 팀에서 존중받고 있다는 느낌이 없다", IsPositive = false },
            new QuestionItem { Category = "관계 스트레스", QuestionText = "가족이나 가까운 사람과도 갈등이 많다", IsPositive = false },
            new QuestionItem { Category = "관계 스트레스", QuestionText = "주변 사람들이 내 고민을 잘 이해해주지 않는다", IsPositive = false },
            new QuestionItem { Category = "관계 스트레스", QuestionText = "나와 가까운 사람들도 내가 지쳤다는 걸 모른다", IsPositive = false },
            new QuestionItem { Category = "자기정체감/불확실성 상태", QuestionText = "나는 내가 어떤 사람인지 점점 헷갈린다", IsPositive = false },
            new QuestionItem { Category = "자기정체감/불확실성 상태", QuestionText = "나는 내가 정말 원하는 게 뭔지 잘 모르겠다", IsPositive = false },
            new QuestionItem { Category = "자기정체감/불확실성 상태", QuestionText = "지금 나는 멈춰 있고, 어디로 가야 할지 모르겠다", IsPositive = false },
            new QuestionItem { Category = "자기정체감/불확실성 상태", QuestionText = "현재 내 위치가 나의 능력에 비해 부족하다고 느낀다", IsPositive = false },
            new QuestionItem { Category = "자기정체감/불확실성 상태", QuestionText = "나는 지금 삶의 주도권을 잃어버린 것 같다", IsPositive = false },
        };
 
    }
}

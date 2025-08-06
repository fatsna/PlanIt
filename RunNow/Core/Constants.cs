using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunNow.Core
{
    public class Constants
    {
        // 감정분석에 쓰일 질문 구조체
        public enum Enum_emotion
        {
            test = 1, // 감정검사 테스트용
            real = 5 // 실제 감정검사용
        }
        public class QuestionItem
        {
            public string? Category { get; set; }
            public string? QuestionText { get; set; }
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

        public static readonly List<List<string>> categori_result = new List<List<string>>
        {
            //감정 상태
            new List<string>
            {
                "감정 인식과 조절 능력이 크게 저하된 상태입니다. 정서적 소진이 심각하여 즉각적인 개입과 휴식이 필요합니다.",
                "감정 상태가 전반적으로 불안정하며, 감정 해소와 지지 시스템 마련이 시급합니다.",
                "감정적으로 지친 상태로, 감정 표현과 회복 활동이 필요한 시기입니다. 소진을 방지하는 조율이 필요합니다.",
                "감정 조절 능력이 점차 회복되고 있으나, 스트레스 요인에 민감할 수 있어 꾸준한 관리가 필요합니다.",
                "감정 인식과 표현이 안정적으로 이루어지고 있습니다. 스스로 조절이 가능한 건강한 감정 상태입니다.",
                "긍정적인 감정 에너지와 안정된 감정 조절 능력을 보여주고 있습니다. 지속적인 자기 관리로 이 상태를 유지하세요.",
                "감정 인식과 조율 능력이 뛰어나며, 일상에서 감정을 건강하게 활용하고 있습니다. 좋은 상태입니다.",
                "최상의 감정 상태입니다. 감정 인식, 표현, 조절까지 매우 건강하게 유지하고 있으며, 타인에게도 긍정적 영향을 미칠 수 있는 수준입니다."
            },
            //이직 욕구/동기 상태
            new List<string>
            {
                "현재 업무에 대한 만족도가 매우 높으며, 이직 욕구는 거의 없습니다.",
                "안정된 업무 환경 속에서 성장과 만족을 느끼고 있습니다.",
                "업무에 대한 관심과 동기가 유지되고 있지만, 가끔 변화 욕구가 나타날 수 있습니다.",
                "업무 만족도가 다소 낮아지고 있으며, 새로운 도전에 대한 욕구가 커지고 있습니다.",
                "현재 업무에서 의미를 찾기 어렵고, 이직을 진지하게 고민하는 단계입니다.",
                "이직 욕구가 강하게 나타나며, 새로운 환경을 찾아야 할 필요성이 높습니다.",
                "현 직무에 대한 불만이 극대화된 상태입니다. 구체적인 이직 준비가 필요합니다.",
                "더 이상 현재 직장에 머무르기 어려운 상황입니다. 빠른 시일 내 변화가 요구됩니다."
            },
            //번아웃/스트레스 지수
            new List<string>
            {
                "스트레스 요인이 거의 없는 안정적인 상태입니다. 현 상태를 유지하는 것이 중요합니다.",
                "스트레스 수준이 낮아 일상생활에서 큰 어려움 없이 활동 중입니다.",
                "가벼운 스트레스가 있지만 자율적으로 관리 가능한 수준입니다.",
                "스트레스 누적이 시작되고 있습니다. 관리와 점검이 필요한 시기입니다.",
                "스트레스로 인한 피로감이 증가하며, 번아웃 초기 증상이 나타날 수 있습니다.",
                "스트레스가 업무/생활 전반에 영향을 미치고 있어 적극적인 스트레스 해소가 필요합니다.",
                "번아웃 상태가 심각해지고 있으며, 긴급한 휴식과 환경 조정이 필요합니다.",
                "심각한 번아웃 상태로, 즉각적인 휴직이나 전문가 상담이 권장됩니다."
            },
            //회복탄력성 / 에너지 상태
            new List<string>
            {
                "에너지와 회복탄력성이 매우 낮은 상태로, 휴식과 에너지 충전이 시급합니다.",
                "일상적인 회복력이 다소 저하된 상태입니다. 작은 성공 경험이 필요합니다.",
                "평균적인 회복탄력성을 유지하고 있으나, 에너지 소모를 주의해야 합니다.",
                "에너지 충전이 필요한 시점입니다. 의식적인 자기 관리가 필요합니다.",
                "기본적인 회복력이 있으나, 급격한 에너지 소모에는 취약할 수 있습니다.",
                "회복탄력성이 좋은 상태이며, 대부분의 스트레스 상황을 잘 견뎌낼 수 있습니다.",
                "높은 회복력을 유지하고 있으며, 도전적 상황에서도 안정적입니다.",
                "최상의 에너지 상태로, 어려운 상황도 긍정적으로 극복할 수 있습니다."
            },
            //가치/비전 일치도
            new List<string>
            {
                "가치와 비전이 현재 직장과 크게 일치하지 않는 상태입니다. 재평가가 필요합니다.",
                "조직의 가치와 나의 가치가 충돌하고 있어, 심리적 불편함이 존재합니다.",
                "조직 문화와 개인 가치 간의 괴리가 커지고 있습니다. 변화가 필요합니다.",
                "조직의 방향성과 나의 목표가 다소 일치하나, 개선 여지가 있습니다.",
                "조직과 개인 가치가 어느 정도 일치하며, 긍정적인 관계를 유지하고 있습니다.",
                "조직의 비전과 나의 가치가 잘 맞아떨어지는 상태입니다. 좋은 조화를 이루고 있습니다.",
                "조직과 개인 가치가 완벽하게 일치하며, 상호 발전이 가능한 상태입니다.",
                "조직과 개인 가치가 완벽하게 일치하여, 최고의 시너지를 낼 수 있는 상태입니다."
            },
            //관계 스트레스
            new List<string>
            {
                "관계 스트레스가 거의 없는 상태입니다. 건강한 대인 관계를 유지하고 있습니다.",
                "대인 관계에서 큰 갈등이나 스트레스가 없으며, 안정적인 관계를 형성하고 있습니다.",
                "일부 관계에서 작은 갈등이 있으나, 관리 가능한 수준입니다.",
                "관계 스트레스가 증가하고 있으며, 소통과 이해가 필요한 시점입니다.",
                "관계에서 심리적 안전감이 부족하며, 개선이 필요합니다.",
                "관계 스트레스가 심화되고 있어, 전문적인 도움이나 조정이 필요합니다.",
                "관계에서 심각한 갈등이 발생하고 있으며, 즉각적인 해결이 요구됩니다.",
                "관계 스트레스가 극대화되어 있으며, 전문가의 개입이 필수적입니다."
            },
            //자기정체감/불확실성 상태
            new List<string>
            {
                "자기 정체감 상실로 인해 일상생활에 큰 어려움을 겪고 있으며, 전문적인 치료가 필수적입니다.",
                "자기 정체감 상실로 인해 심각한 심리적 고통을 겪고 있습니다. 즉각적인 개입이 필요합니다.",
                "자기 정체감이 크게 흔들리고 있으며, 전문적인 상담이 필요합니다.",
                "자기 정체감이 약해지고 있으며, 심리적 불안정성이 나타납니다.",
                "자기 정체감에 대한 의문이 생기기 시작하며, 탐색이 필요한 시점입니다.",
                "자기 정체감이 어느 정도 확립되어 있으나, 가끔 혼란스러울 수 있습니다.",
                "자신의 가치와 목표가 뚜렷하여, 삶에 대한 확신이 있습니다.",
                "자기 정체감이 매우 확고하며, 삶의 방향성이 명확합니다."
            }
        };

    }
}

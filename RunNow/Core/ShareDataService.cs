using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json.Linq;

namespace RunNow.Core
{
    public partial class ShareDataService : ObservableObject
    {
        // 둘다 김대업이 임시로 써놨음
        public string? User_id { get; set; }
        public string Password { get; set; } = string.Empty;

        // 성장플래닛 시작하기 맴버
        public List<string>? Jobs { get; set; } // 추천 & 희망 직업 리스트
        public List<string>? Jobs_EXPLAIN { get; set; }  // 추천 & 희망 직업 설명 리스트
        public List<string>? Jobs_REASON { get; set; }  // 추천 & 희망 직업 이유 리스트
        [ObservableProperty] public bool isDetailVisible = false; // 뒤로가기 메시지

        [ObservableProperty] public int period = 0; // 성장플래닛 예상기간
        [ObservableProperty] public string wantjob = ""; // 성장플래닛 목표 직업
        //[ObservableProperty] private DateOnly START_DAY; // 현재 목표 인덱스
        public ObservableCollection<GoalDisplay> Goals { get; set; } // 목표들!
        public class GoalDisplay
        {
            public string Category { get; set; } // 카테고리
            public string Goal { get; set; } // 목표이름
            public string Date { get; set; } // 완료 날짜 / 없으면 ""
            public int Importance { get; set; } // 중요도 1 ~ 9
            public int Goal_Progress { get; set; } // 달성도 비중 %

            public bool IsSelected = false;  // ← 선택 상태
        };
        public string RES_ID { get; set; } // 종합테스트 번호 저장하기
        public string Growth_ID { get; set; } // 성장플래닛 번호 저장하기

    }
}

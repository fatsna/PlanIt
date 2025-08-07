using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunNow.Core
{
    public class ShareDataService
    {
        // 둘다 김대업이 임시로 써놨음
        public string ? User_id { get; set; }
        public string Password { get; set; } = string.Empty;

        // 성장플래닛 시작하기 맴버
        public List<string> ?  Jobs { get; set; }
        public List<string> ? Jobs_EXPLAIN { get; set; }

        // 성장플래닛 채우기, 마이플래닛 맴버

    }
}

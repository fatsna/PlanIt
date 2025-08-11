using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RunNow.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string username, string password);
        Task<bool> FaceLoginAsync();
        Task<JObject> PlanIT_start(string User_id); // 플래닛 시작하기 눌렀을때 종합테스트 유무 확인
        Task<JObject> PlanIT_check(string User_id); // 플래닛 채우기, 마이플래닛 눌렀을때 성장플래닛 유무 확인
        Task<JObject> PlanIT_serch(string job); // 직업검색
        Task<JObject> PlanIT_make(string job, string id); // 성장 플래닛 만들기
        Task<JObject> PlanIT_goal(string id, int GROWN_ID, List<string> GOAL); // 목표달성! 
    }
}

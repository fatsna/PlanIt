using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RunNow.Services
{
    public interface IAuthService
    {
        Task<JObject> LoginAsync(string username, string password);
        //얼굴인식 로그인
        Task<JObject> FaceLoginAsync(float[] embedding);
        //회원가입
        Task<JObject> RegisterAsync(JObject registerPayload);
        //중복확인 
        Task<JObject> CheckDuplicateIdAsync(string userid);
        Task<JObject> PlanIT_start(string User_id); // 플래닛 시작하기 눌렀을때 종합테스트 유무 확인
        Task<JObject> PlanIT_check(string User_id); // 플래닛 채우기, 마이플래닛 눌렀을때 성장플래닛 유무 확인
        Task<JObject> PlanIT_serch(string job); // 직업검색


        //이력서 보내기 
        Task<JObject> SaveResumeAsync(JObject Payload);
        Task<JObject> QueryResumeAsync(JObject payload);//이력서 조회

        Task<JObject> GetMyPageAsync(string uid); //  마이페이지(내정보+검사결과)
        Task<JObject> DeepTestAsync(JObject payload);        //정밀테스트 

        Task<JObject> PlanIT_make(string job, string id); // 성장 플래닛 만들기
        Task<JObject> PlanIT_goal(string id, int GROWN_ID, List<string> GOAL, string date); // 목표달성! 

    }
}

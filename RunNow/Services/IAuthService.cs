using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RunNow.Services
{
    public interface IAuthService
    {
        Task<JObject> LoginAsync(string username, string password); //  수정된 부분
        Task<JObject> FaceLoginAsync(float[] embedding);


        Task<JObject> RegisterAsync(JObject registerPayload);

        Task<JObject> CheckDuplicateIdAsync(string userid);

    }
}

using System.Threading.Tasks;

namespace RunNow.Services
{
    public class AuthService : IAuthService
    {
        private readonly TcpClientService _tcpClientService;

        public AuthService(TcpClientService tcpClientService)
        {
            _tcpClientService = tcpClientService;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var json = new Newtonsoft.Json.Linq.JObject
            {
                ["action"] = "login",
                ["username"] = username,
                ["password"] = password
            };

            var response = await _tcpClientService.SendJsonToServer(json);
            return response["status"]?.ToString() == "success";
        }

        public async Task<bool> FaceLoginAsync()
        {
            var json = new Newtonsoft.Json.Linq.JObject
            {
                ["action"] = "face_login"
            };

            var response = await _tcpClientService.SendJsonToServer(json);
            return response["status"]?.ToString() == "success";
        }
    }
}

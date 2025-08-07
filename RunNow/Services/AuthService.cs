<<<<<<< HEAD
﻿using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
=======
﻿using RunNow.Core;
using System.Threading.Tasks;
>>>>>>> 00e8723dd4f6cbad5e5e3b1d2d6e3ef3252b8926

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

        public async Task<JObject> PlanIT_start(string User_id) // 플래닛 시작하기 눌렀을때 종합테스트 유무 확인
        {
            JObject json = new JObject()
            {
                ["protocol"] = "5_0",
                ["user_id"] = User_id
            };

            var response = await this._tcpClientService.SendJsonToServer(json);
            return response;
        }
        public async Task<JObject> PlanIT_check(string User_id) // 플래닛 채우기, 마이플래닛 눌렀을때 성장플래닛 유무 확인
        {
            JObject json = new JObject()
            {
                ["protocol"] = "5_0",
                ["user_id"] = User_id
            };

            var response = await this._tcpClientService.SendJsonToServer(json);
            return response;
        }
        public async Task<JObject> PlanIT_serch(string job)
        {
            JObject json = new JObject()
            {
                ["protocol"] = "100_5_0",
                ["job"] = job
            };
            JObject response = await this._tcpClientService.SendJsonToServer(json);
            return response;
        }
        public async Task<JObject> PlanIT_make(string job)
        {
            JObject json = new JObject()
            {
                ["protocol"] = "100_6_0",
                ["job"] = job
            };
            JObject response = await this._tcpClientService.SendJsonToServer(json);
            return response;
        }
    }
}

using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
﻿using RunNow.Core;
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

        public async Task<JObject> PlanIT_start(string User_id) // 플래닛 시작하기 눌렀을때 종합테스트 유무 확인
        {
            JObject json = new JObject()
            {
                ["protocol"] = "5_0",
                ["u_id"] = User_id
            };

            var response = await this._tcpClientService.SendJsonToServer(json);
            return response;
        }
        public async Task<JObject> PlanIT_check(string User_id) // 플래닛 채우기, 마이플래닛 눌렀을때 성장플래닛 유무 확인
        {
            JObject json = new JObject()
            {
                ["protocol"] = "9_0",
                ["u_id"] = User_id
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
        public async Task<JObject> PlanIT_make(string job, string id)
        {
            JObject json = new JObject()
            {
                ["protocol"] = "100_6_0",
                ["u_id"] = id,
                ["job"] = job
            };
            JObject response = await this._tcpClientService.SendJsonToServer(json);
            return response;
        }
        public async Task<JObject> PlanIT_goal(string id, int GROWN_ID, string GOAL) 
        {
            JObject json = new JObject()
            {
                ["protocol"] = "10_0",
                ["u_id"] = id,
                ["GROWN_ID"] = GROWN_ID,
                ["GOAL"] = GOAL,
                ["GOAL_DATE"] = DateTime.Now.ToString("yyyy-MM-dd") // 날짜만 전송
            };
            JObject response = await this._tcpClientService.SendJsonToServer(json);
            return response;
        }
    }
}

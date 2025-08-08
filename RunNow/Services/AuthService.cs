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
        //로그인
        public async Task<JObject> LoginAsync(string username, string password)
        {
            await _tcpClientService.ConnectAsync();  // 서버 연결 보장

            JObject json = new JObject
            {
                ["protocol"] = "1_0",
                ["id"] = username,
                ["pw"] = password
            };

            var response = await _tcpClientService.SendJsonToServer(json);

            return response;  // 그대로 반환 (ViewModel이 판단할 수 있게)
        }

        //얼굴 인식 로그인
        public async Task<JObject> FaceLoginAsync(float[] embedding)
        {
            if (embedding == null || embedding.Length == 0)
            {
                return JObject.FromObject(new
                {
                    status = "error",
                    message = "임베딩 값이 없습니다."
                });
            }

            // 🔥 얼굴인식 로그인 
            await _tcpClientService.ConnectAsync();

            var json = new JObject
            {
                ["protocol"] = "2_0",
                ["face_id"] = new JObject
                {
                    ["embedding"] = new JArray(embedding)
                }
            };

            Console.WriteLine("📡 FaceLoginAsync → 서버 전송 직전 JSON:");
            Console.WriteLine(json.ToString());

            var response = await _tcpClientService.SendJsonToServer(json);
            return response;
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

        //아이디 중복검사
        public async Task<JObject> CheckDuplicateIdAsync(string userid)
        {
            await _tcpClientService.ConnectAsync();

            JObject payload = new JObject
            {
                ["protocol"] = "3_0",
                ["id"] = userid
            };

            var response = await _tcpClientService.SendJsonToServer(payload);
            return response;
        }


        //회원가입 
        public async Task<JObject> RegisterAsync(JObject registerPayload)
        {
            await _tcpClientService.ConnectAsync();  // 서버 연결

            var response = await _tcpClientService.SendJsonToServer(registerPayload);

            return response;
        }

    }
}

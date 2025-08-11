using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RunNow.Core;

namespace RunNow.Services
{
    public class AuthService : IAuthService
    {
        private readonly TcpClientService _tcpClientService;
        private readonly ShareDataService _share;   // ✅ 주입받아 사용

        // ✅ 생성자에 ShareDataService 추가
        public AuthService(TcpClientService tcpClientService, ShareDataService share)
        {
            _tcpClientService = tcpClientService;
            _share = share;
        }

        // 로그인
        public async Task<JObject> LoginAsync(string username, string password)
        {
            await _tcpClientService.ConnectAsync();

            var json = new JObject
            {
                ["protocol"] = "1_0",
                ["id"] = username,
                ["pw"] = password
            };

            return await _tcpClientService.SendJsonToServer(json);
        }

        // 얼굴 인식 로그인
        public async Task<JObject> FaceLoginAsync(float[] embedding)
        {
            if (embedding == null || embedding.Length == 0)
                return JObject.FromObject(new { status = "error", message = "임베딩 값이 없습니다." });

            await _tcpClientService.ConnectAsync();

            var json = new JObject
            {
                ["protocol"] = "2_0",
                ["face_id"] = new JObject { ["embedding"] = new JArray(embedding) }
            };

            Console.WriteLine("[FaceLogin REQ] " + json.ToString());
            return await _tcpClientService.SendJsonToServer(json);
        }

        // 플래닛 시작 여부 확인
        public async Task<JObject> PlanIT_start(string userId)
        {
            await _tcpClientService.ConnectAsync();

            var json = new JObject
            {
                ["protocol"] = "5_0",
                ["user_id"] = userId
            };

            return await _tcpClientService.SendJsonToServer(json);
        }

        // 플래닛 채우기/마이플래닛 확인
        public async Task<JObject> PlanIT_check(string userId)
        {
            await _tcpClientService.ConnectAsync();

            var json = new JObject
            {
                ["protocol"] = "5_0",
                ["user_id"] = userId
            };

            return await _tcpClientService.SendJsonToServer(json);
        }

        public async Task<JObject> PlanIT_serch(string job)
        {
            await _tcpClientService.ConnectAsync();

            var json = new JObject
            {
                ["protocol"] = "100_5_0",
                ["job"] = job
            };

            return await _tcpClientService.SendJsonToServer(json);
        }

        public async Task<JObject> PlanIT_make(string job)
        {
            await _tcpClientService.ConnectAsync();

            var json = new JObject
            {
                ["protocol"] = "100_6_0",
                ["job"] = job
            };

            return await _tcpClientService.SendJsonToServer(json);
        }

        // 아이디 중복 검사
        public async Task<JObject> CheckDuplicateIdAsync(string userid)
        {
            await _tcpClientService.ConnectAsync();

            var payload = new JObject
            {
                ["protocol"] = "3_0",
                ["id"] = userid
            };

            return await _tcpClientService.SendJsonToServer(payload);
        }

        // 회원가입
        public async Task<JObject> RegisterAsync(JObject registerPayload)
        {
            await _tcpClientService.ConnectAsync();
            return await _tcpClientService.SendJsonToServer(registerPayload);
        }

        // 마이페이지
        public async Task<JObject> GetMyPageAsync(string uid)
        {
            await _tcpClientService.ConnectAsync();

            var req = new JObject
            {
                ["protocol"] = "8_0",
                ["U_ID"] = uid
            };

            return await _tcpClientService.SendJsonToServer(req);
        }

        // 정밀테스트: 최상위에 U_ID 포함해서 전송
        public async Task<JObject> DeepTestAsync(JObject payload)
        {
            await _tcpClientService.ConnectAsync();

            var uId = _share?.User_id ?? _share?.CurrentUserId ?? string.Empty;

            var req = new JObject
            {
                ["protocol"] = "100_0_0",
                ["U_ID"] = uId
            };

            // payload의 키를 최상위로 병합
            foreach (var p in payload.Properties())
                req[p.Name] = p.Value;

            Console.WriteLine("[DeepTest REQ] " + req.ToString());
            return await _tcpClientService.SendJsonToServer(req);
        }
    }
}

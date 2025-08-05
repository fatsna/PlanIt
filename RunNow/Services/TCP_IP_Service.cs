using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace RunNow.Services
{
    internal class TCP_IP_Service
    {

        private TcpClient client;   //서버와 연결을 관리하는 객체
        private NetworkStream stream;   //서버와 실제 데이터를 주고받는 통로 
        public bool IsConnected => client?.Connected ?? false;

        public async Task ConnectAsync(string ip = "10.10.20.96", int port = 9034)
        {
            if (IsConnected) return;

            client = new TcpClient();
            await client.ConnectAsync(ip, port);
            stream = client.GetStream();
        }

        public async Task<JObject> SendJsonToServer(JObject message)
        {
            var payload = new
            {
                message = "서버에 연결되어 있지 않습니다."
            };

            // 서버 연결 확인
            if (!IsConnected)
            {
                return JObject.FromObject(payload);
            }

            try
            {
                //  전송할 JSON 만들기
                var Message = message;
                string jsonString = JsonConvert.SerializeObject(Message);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);

                // 1. 길이를 4바이트로 변환 (Little Endian)
                byte[] lengthPrefix = BitConverter.GetBytes(jsonBytes.Length);

                // 2. 최종 전송할 바이트 배열 만들기 (길이 + 본문)
                byte[] dataToSend = new byte[lengthPrefix.Length + jsonBytes.Length];
                Buffer.BlockCopy(lengthPrefix, 0, dataToSend, 0, lengthPrefix.Length);        // Length 붙이기
                Buffer.BlockCopy(jsonBytes, 0, dataToSend, lengthPrefix.Length, jsonBytes.Length);  // JSON 붙이기

                Console.WriteLine($"\n[서버 전송] Length: {jsonBytes.Length}, JSON: {jsonString}");

                //  서버로 전송
                await stream.WriteAsync(dataToSend, 0, dataToSend.Length);
                Console.WriteLine(" 전송 완료, 응답 대기 중...");

                // 1. Length Prefix(4바이트) 먼저 읽기
                byte[] header = new byte[4];
                int totalHeaderRead = 0;
                while (totalHeaderRead < 4)
                {
                    int bytesRead = await stream.ReadAsync(header, totalHeaderRead, 4 - totalHeaderRead);
                    if (bytesRead == 0)
                    {
                        throw new Exception("서버와의 연결이 끊어졌습니다 (Header)");
                    }
                    totalHeaderRead += bytesRead;
                }

                // Length 값 구하기 (Little Endian)
                int bodyLength = BitConverter.ToInt32(header, 0);
                Console.WriteLine($"[DEBUG] 수신할 본문 길이: {bodyLength} 바이트");

                // 2. 본문 데이터(Read Body)
                byte[] body = new byte[bodyLength];
                int totalBodyRead = 0;
                while (totalBodyRead < bodyLength)
                {
                    int bytesRead = await stream.ReadAsync(body, totalBodyRead, bodyLength - totalBodyRead);
                    if (bytesRead == 0)
                    {
                        throw new Exception("서버와의 연결이 끊어졌습니다 (Body)");
                    }
                    totalBodyRead += bytesRead;
                }

                // 3. JSON 문자열로 디코딩
                string receivedJson = Encoding.UTF8.GetString(body);
                Console.WriteLine($"[서버 응답] {receivedJson}");

                // 4. 필요시 JObject로 파싱
                var jsonResponse = JObject.Parse(receivedJson);
                // 5. 문자열로 쓰려면
                string result = jsonResponse.ToString();
                Console.WriteLine($" [서버 응답] {result}");

                // 응답 분기 처리
                return jsonResponse;
            }
            catch (Exception ex)
            {
                return JObject.FromObject(payload);
            }
        }
    }

}

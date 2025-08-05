using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace ByeCompany
{
    /// <summary>
    /// login.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class login : Page
    {
        private TcpListener listener;

        public login()
        {
            InitializeComponent();
            StartSocketServer(); // 페이지 시작 시 서버 가동
        }

        private void StartSocketServer()
        {
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 9999);
            listener.Start();

            Thread serverThread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        NetworkStream stream = client.GetStream();

                        byte[] buffer = new byte[1024];
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        if (message == "FACE_DETECTED")
                        {
                            // UI 스레드에서 실행
                            Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show("✅ 얼굴 인식 성공! 로그인합니다.");

                                // 페이지 전환 예시
                                // NavigationService?.Navigate(new MainPage());
                            });
                        }

                        client.Close();
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show("서버 오류: " + ex.Message);
                        });
                    }
                }
            });

            serverThread.IsBackground = true;
            serverThread.Start();
        }

        // 👇 버튼 클릭 시 Python에 신호 보내기
        private void FaceDetectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (TcpClient client = new TcpClient("127.0.0.1", 9999)) // Python 서버는 9998번 포트로 가정
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] message = Encoding.UTF8.GetBytes("START_FACE_DETECTION");
                    stream.Write(message, 0, message.Length);

                    MessageBox.Show("📤 얼굴 인식 요청을 Python에 전송했습니다.");
                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show("❌ Python 서버에 연결할 수 없습니다.\n" + ex.Message);
            }
        }
    }
}

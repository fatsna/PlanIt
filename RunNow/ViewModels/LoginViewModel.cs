using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Input;

namespace PlanIt.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public Action FaceDetected { get; set; }

        private string _username;
        private string _password;
        private TcpListener _listener;
        private float[] _faceEmbedding;

        private static bool serverStarted = false;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(nameof(Username)); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }

        public float[] FaceEmbedding
        {
            get => _faceEmbedding;
            set { _faceEmbedding = value; OnPropertyChanged(nameof(FaceEmbedding)); }
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);

            if (!serverStarted)
            {
                StartSocketServer();
                serverStarted = true;
            }
        }

        private void StartSocketServer()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 5001);
                _listener.Start();

                Thread serverThread = new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            using (TcpClient client = _listener.AcceptTcpClient())
                            using (NetworkStream stream = client.GetStream())
                            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                            {
                                string message = reader.ReadToEnd().Trim();

                                if (!string.IsNullOrWhiteSpace(message) && message.StartsWith("{"))
                                {
                                    var result = JObject.Parse(message);
                                    var embeddingArray = (JArray)result["embedding"];
                                    FaceEmbedding = embeddingArray.ToObject<float[]>();

                                    FaceDetected?.Invoke();  // ✅ UI 업데이트 등
                                }
                                else
                                {
                                    // 필요시 기본 메시지 처리 (예: 디버그 출력)
                                    Console.WriteLine("받은 메시지: " + message);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("소켓 처리 중 예외 발생: " + ex.Message);
                        }
                    }
                });

                serverThread.IsBackground = true;
                serverThread.Start();
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"소켓 오류 발생: {ex.Message}");
            }
        }


        private void ExecuteLogin(object parameter)
        {
            // 로그인 처리 로직
        }
    }

    public class FaceRecognitionResult
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("embedding")]
        public float[] Embedding { get; set; }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}

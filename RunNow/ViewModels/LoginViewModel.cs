using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Input;

namespace PlanIt.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        public event Action FaceDetected;

        private string _username;
        private string _password;
        private TcpListener _listener;

        // ✅ 서버 중복 실행 방지용 플래그
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

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);

            // ✅ 서버는 한 번만 실행되도록 처리
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
                            TcpClient client = _listener.AcceptTcpClient();
                            NetworkStream stream = client.GetStream();

                            byte[] buffer = new byte[1024];
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);
                            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                            if (message.ToUpper() == "FACE_DETECTED")
                            {
                                FaceDetected?.Invoke();
                            }
                            else
                            {
                                Username = message;
                            }
                        }
                        catch
                        {
                            break;
                        }
                    }
                });

                serverThread.IsBackground = true;
                serverThread.Start();
            }
            catch (SocketException ex)
            {
                // 예외 처리 (로그 또는 사용자에게 알림 등)
                Console.WriteLine($"소켓 오류 발생: {ex.Message}");
            }
        }

        private void ExecuteLogin(object parameter)
        {
            // 로그인 처리 로직 구현
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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

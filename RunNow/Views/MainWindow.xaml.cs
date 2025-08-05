using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using RunNow.Core;
using RunNow.Models;
using RunNow.Services;
using RunNow.ViewModels;

namespace RunNow.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // 통신클래스..
        private readonly TCP_IP_Service tcp;
        public MainWindow()
        {
            InitializeComponent();
            tcp = App.ServiceProvider.GetRequiredService<TCP_IP_Service>();
            tcp.ConnectAsync();  // 비동기 연결 시도

            var test_message = new
            {
                message = "서버님 안녕하세요.",
                test = "화이팅입니다",
                hi = true,
                count = 100
            };
            // 서버랑 통신용
            tcp.SendJsonToServer(JObject.FromObject(test_message));

            EmotionViewModel vm = new EmotionViewModel(Constants.Enum_emotion.test);  // 파라미터 전달
            var view = new EmotionView(); // View 생성
            view.DataContext = vm; // ViewModel과 연결
                                   // MainFrame에 View를 설정
            Console.WriteLine($"Initializing EmotionViewModel...?!??");
            this.MainFrame.Navigate(view);
            JObject OnEmotionResult;
            MessengerService.Register<JObject>(this, result =>
            {
                var vm = new EmotionResultViewModel(result);
                var view = new EmotionResultView { DataContext = vm };
                MainFrame.Navigate(view);
            });

            // 이건 액션 시그널슬롯 위에건 메신져무ㅗㅓ시기
            //    vm.Result += (EmotionResult) =>
            //    {
            //        OnEmotionResult = EmotionResult;
            //        Console.WriteLine($"[DEBUG] EmotionResult: {string.Join(", ", OnEmotionResult.Properties().Select(kv => $"{kv.Name}: {kv.Value}"))}");
            //        // 결과를 보여줄 때 사용할 Action 등록
            //        // 여기서 OnEmotionResult를 사용하여 결과를 처리할 수 있습니다.
            //        // 예: MessageBox.Show($"감정 분석 결과: {string.Join(", ", OnEmotionResult.Select(kv => $"{kv.Key}: {kv.Value}"))}");



            //        //Emotion_result_model model = new Emotion_result_model();
            //        //model = OnEmotionResult.ToObject<Emotion_result_model>();


            //        ////var vm = new EmotionViewModel(Constants.Enum_emotion.real);  // 파라미터 전달
            //        //var view2 = new EmotionResultView(); // View 생성
            //        //view2.DataContext = vm; // ViewModel과 연결

            //        //this.MainFrame.Navigate(view2);
            //    };
        }
    }
}
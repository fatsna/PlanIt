using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using RunNow.Core;
using RunNow.Services;

namespace RunNow.ViewModels
{
    internal partial class Growth_main_ViewModel : ObservableObject
    {
        private readonly NavigationStore _navigationStore; // 메인에게 화면넘겨! 객체
        private readonly IAuthService _authService;         // 서버와 통신 객체
        public ShareDataService shareDataService;    // 유저 데이터 객체
        private readonly IServiceProvider _serviceProvider;   // 서비스 프로바이더 객체
        private bool check = true; // 확인용 변수
        private JObject? json = null; // 서버응답 변수
        public Growth_main_ViewModel(NavigationStore navigationStore, IAuthService authService, IServiceProvider serviceProvider, ShareDataService shareDataService, TcpClientService tcpClientService)
        {
            // 통신용 클래스 객체
            this._authService = authService;
            // 유저의 모든정보 서비스 객체
            this.shareDataService = shareDataService;
            // 메인에게 페이지 전환알리는 객체
            this._navigationStore = navigationStore;
            this._serviceProvider = serviceProvider;
            //@@@@@@@@@@@가짜아이디붙여보내기
            tcpClientService.ConnectAsync();
            this.shareDataService.User_id = "admin";
            //@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
            Console.WriteLine($"{this.shareDataService.IsDetailVisible}");

        }

        [RelayCommand] private async Task Planit_start()
        {
            //// 종합테스트 결과가 있다면 페이지전환 아니면 그냥있기~
            this.json = await this._authService.PlanIT_start(this.shareDataService.User_id);
            Console.WriteLine($"서버가 보내준값! : {this.json.ToString()}");
            if (this.check = (this.json["protocol"]?.ToString() == "5_1"))
            {
                Console.WriteLine("플래닛 시작!!");
                List<string> jobs = new List<string>(); // 직업들
                List<string> descriptions = new List<string>(); // 직업 설명들
                List<string> reason = new List<string>(); // 추천이유
                Console.WriteLine("직업받기 시작!!");

                foreach (var item in this.json["items"])
                {
                    Console.WriteLine($"저장하는 직업정보!{item}");
                    jobs.Add(item["job"].ToString());
                    descriptions.Add(item["description"].ToString());
                    reason.Add(item["reason"].ToString());
                }
                Console.WriteLine("직업받기 끝!!!");
                this.shareDataService.Jobs = jobs;
                this.shareDataService.Jobs_EXPLAIN = descriptions;
                this.shareDataService.Jobs_REASON = reason;
                this._navigationStore.CurrentViewModel = this._serviceProvider.GetRequiredService<growth_start_ViewModel>();

            }
            else
            {
                // 테스트결과가 없음 종합결과 만드실래요?~!
                this.check = true;
                this.shareDataService.IsDetailVisible = true;
            }
        }

        [RelayCommand] private async Task Planit_check()
        {
            Console.WriteLine("첵플래닛!!");

            // 성장플래닛 결과가없으면 페이지전환 아니면 그냥있기~
            this.json = await this._authService.PlanIT_check(this.shareDataService.User_id);
            Console.WriteLine($"서버가 보내준값! : {this.json.ToString()}");
            if (this.check = (this.json["protocol"]?.ToString() == "9_1"))
            {
            //    this.json에 받은 데이터를 this._shareDataService << 에 저장
            // services.AddSingleton<ShareDataService>(); << 이게 인스턴스를 싱글톤으로 등록한것
            //✅ 클래스와 인스턴스 = 참조 타입 → "주소값을 가진다"
            //     C#에서 class로 선언된 모든 객체는 힙(heap) 메모리에 저장되고,
            //     변수에는 그 객체가 저장된 메모리 주소(참조값) 만 저장됩니다.
                Console.WriteLine("플래닛 체크!!!");
                this._navigationStore.CurrentViewModel = this._serviceProvider.GetRequiredService<growth_check_ViewModel>();
            }
            else
            {
                // 테스트결과가 없음 플래닛만들기나 종합결과를 유도
                this.check = true;
                this.shareDataService.IsDetailVisible = true;
            }
        }

        [RelayCommand] private async Task Planit_My()
        {
            Console.WriteLine("마이 플래닛!!");

            // 성장플래닛 결과가없으면 페이지전환 아니면 그냥있기~
            this.json = await this._authService.PlanIT_check(this.shareDataService.User_id);
            Console.WriteLine($"서버가 보내준값! : {this.json.ToString()}");
            if (this.check = (this.json["protocol"]?.ToString() == "9_1"))
            {
                 //   this.json에 받은 데이터를 this._shareDataService << 에 저장
                 //services.AddSingleton<ShareDataService>(); << 이게 인스턴스를 싱글톤으로 등록한것
                 //C# 에서 클래스는 참조공유다!
                this._navigationStore.CurrentViewModel = this._serviceProvider.GetRequiredService<Growth_My_ViewModel>();
            }
            else
            {
                // 테스트결과가 없음 플래닛만들기나 종합결과를 유도
                this.check = true;
                this.shareDataService.IsDetailVisible = true;
            }
        }
        [RelayCommand] private void Back()
        {
            this._navigationStore.CurrentViewModel = this._serviceProvider.GetRequiredService<MainViewModel>();
        }
        [RelayCommand] private void RectangleClick()
        {
            this.shareDataService.IsDetailVisible = false;
            Console.WriteLine($"{this.shareDataService.IsDetailVisible}");
        }

    }
}

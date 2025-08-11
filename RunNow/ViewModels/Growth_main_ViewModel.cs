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
        public Growth_main_ViewModel(NavigationStore navigationStore, IAuthService authService, IServiceProvider serviceProvider, ShareDataService shareDataService)
        {
            // 통신용 클래스 객체
            this._authService = authService;
            // 유저의 모든정보 서비스 객체
            this.shareDataService = shareDataService;
            // 메인에게 페이지 전환알리는 객체
            this._navigationStore = navigationStore;
            this._serviceProvider = serviceProvider;
        }

        [RelayCommand] private async Task Planit_start()
        {
            //// 종합테스트 결과가 있다면 페이지전환 아니면 그냥있기~
            //this.json = await this._authService.PlanIT_start(this._shareDataService.User_id);
            //if( this.check = (this.json["protocol"]?.ToString() == "110_1"))
            //{
            Console.WriteLine("플래닛 시작!!");

            List<string> tmp = new List<string>() { "개발자", "디자이너", "마케터", "개발자1", "디자이1너", "마케1터", "개발2자", "디자이2너", "마2케터" };
            List<string> tmp2 = new List<string>() { "컴퓨터 소프트웨어와 애플리케이션을 설계하고 구현하며, 시스템 유지보수와 오류 수정도 수행합니다.",
                                                     "시각적 요소를 기획하고 디자인하여 사용자 경험을 개선하며, 브랜드 이미지를 시각적으로 표현합니다.",
                                                     "시장을 분석하고 소비자 니즈를 파악하여 효과적인 전략으로 제품이나 서비스를 홍보하고 판매를 촉진합니다.", "컴퓨터 소프트웨어와 애플리케이션을 설계하고 구현하며, 시스템 유지보수와 오류 수정도 수행합니다.222222",
                                                     "시각적 요소를 기획하고 디자인하여 사용자 경험을 개선하며, 브랜드 이미지를 시각적으로 표현합니다.222222222",
                                                     "시장을 분석하고 소비자 니즈를 파악하여 효과적인 전략으로 제품이나 서비스를 홍보하고 판매를 촉진합니다2222222222222222.", "컴퓨터 소프트웨어와 애플리케이션을 설계하고 구현하며, 시스템 유지보수와 오류 수정도 수행합니다.123123",
                                                     "시각적 요소를 기획하고 디자인하여 사용자 경험을 개선하며, 브랜드 이미지를 시각적으로 표현합니다12123.",
                                                     "시장을 분석하고 소비자 니즈를 파악하여 효과적인 전략으로 제품이나 서비스를 홍보하고 판매를 촉진합니다123123."};
            this.shareDataService.Jobs = tmp;
            this.shareDataService.Jobs_EXPLAIN = tmp2;
            this._navigationStore.CurrentViewModel = this._serviceProvider.GetRequiredService<growth_start_ViewModel>();

            //}
            //else
            //{
            //    // 테스트결과가 없음 종합결과 만드실래요?~!
            //  this.check = true
            //}
        }

        [RelayCommand] private async Task Planit_check()
        {
            // 성장플래닛 결과가없으면 페이지전환 아니면 그냥있기~
            //this.json = await this._authService.PlanIT_start(this._shareDataService.User_id);
            //if( this.check = (this.json["protocol"]?.ToString() == "111_1"))
            //{
            // this.json에 받은 데이터를 this._shareDataService << 에 저장
            // services.AddSingleton<ShareDataService>(); << 이게 인스턴스를 싱글톤으로 등록한것
            //✅ 클래스와 인스턴스 = 참조 타입 → "주소값을 가진다"
            //     C#에서 class로 선언된 모든 객체는 힙(heap) 메모리에 저장되고,
            //     변수에는 그 객체가 저장된 메모리 주소(참조값) 만 저장됩니다.
            Console.WriteLine("플래닛 체크!!!");
            this._navigationStore.CurrentViewModel = this._serviceProvider.GetRequiredService<growth_check_ViewModel>();
            //}
            //else
            //{
            //    // 테스트결과가 없음 플래닛만들기나 종합결과를 유도
            //  this.check = true
            //}
        }

        [RelayCommand] private async Task Planit_My()
        {
            Console.WriteLine("마이 플래닛!!");

            // 성장플래닛 결과가없으면 페이지전환 아니면 그냥있기~
            //this.json = await this._authService.PlanIT_start(this._shareDataService.User_id);
            //if( this.check = (this.json["protocol"]?.ToString() == "111_1"))
            //{
            // this.json에 받은 데이터를 this._shareDataService << 에 저장
            // services.AddSingleton<ShareDataService>(); << 이게 인스턴스를 싱글톤으로 등록한것
            // C# 에서 클래스는 참조공유다!
            this._navigationStore.CurrentViewModel = this._serviceProvider.GetRequiredService<Growth_My_ViewModel>();
            //}
            //else
            //{
            //    // 테스트결과가 없음 플래닛만들기나 종합결과를 유도
            //  this.check = true
            //}
        }
        [RelayCommand] private void Back()
        {
            this._navigationStore.CurrentViewModel = this._serviceProvider.GetRequiredService<MainViewModel>();
        }
    }
}

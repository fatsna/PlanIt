using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class growth_start_ViewModel : ObservableObject
    {
        public growth_start_ViewModel(NavigationStore navigationStore, IAuthService authService, IServiceProvider serviceProvider, ShareDataService shareDataService)
        {
            // 매개인자로 받은거 복사하기~
            this._serviceProvider = serviceProvider;
            this._authService = authService;
            this._navigationStore = navigationStore;
            this.shareDataService = shareDataService;
            Console.WriteLine($"생성자에서의 값 : {this.shareDataService.IsDetailVisible}");
            this.SerchJob = "검색은 여기에서 해주세요!";
            this.Wantjobtext = "플래닛 만들기"; // 처음에 써질값.
            // 복사~
            // 기존 string 리스트 → JobItem으로 변환
            this.jobs = new ObservableCollection<JobItem>(
                this.shareDataService.Jobs
                .Select((job, idx) => new JobItem { Name = job, Index = idx })
            );
            this.job_explains = new ObservableCollection<string>(this.shareDataService.Jobs_EXPLAIN);
        }

        private readonly NavigationStore _navigationStore; // 메인에게 화면넘겨! 객체
        private readonly IAuthService _authService;         // 서버와 통신 객체
        public ShareDataService shareDataService { get; set; }    // 유저 데이터 객체
        private readonly IServiceProvider _serviceProvider;   // 서비스 프로바이더 객체
        private readonly string job_picked; // 고른 직업

        [ObservableProperty] private ObservableCollection<JobItem> jobs; // sharedate의 직업들을 복사할 변수
        [ObservableProperty] private ObservableCollection<string> job_explains; // sharedata의 직업설명들을 복사할 변수
        [ObservableProperty] private string selectedJobExplain; // ui와 바인딩될 직업설명 변수
        [ObservableProperty] private string selectedJobReson; // ui와 바인딩될 직업설명 변수
        [ObservableProperty] private string serchJob; // 유저가 검색한 직업
        [ObservableProperty] private string wantjob; // 유저가 고른 직업
        [ObservableProperty] private string wantjobtext; // 
        public class JobItem
        {
            public string Name { get; set; }
            public int Index { get; set; }
        }

        [RelayCommand] private void Back()
        {
            // 무슨방법이 올바른가?

            //_navigationStore.CurrentViewModel = App.Services.GetRequiredService<MainViewModel>();
            Console.WriteLine("뒤로가기버튼 누름!");
            Console.WriteLine($"{this.shareDataService.IsDetailVisible} 값");
            this.shareDataService.IsDetailVisible = true; // 뒤로가기 메시지 보이기
            Console.WriteLine($"{this.shareDataService.IsDetailVisible} 값");

            // 이게 MVVM 패턴에 올바르다!
            //this._navigationStore.CurrentViewModel = this._serviceProvider.GetRequiredService<Growth_main_ViewModel>();
        }

        [RelayCommand] private void SelectJob(int index)
        {
            Console.WriteLine($"직업 누름!!! 누른인덱스 : {index}");
            if (index >= 0 && index < this.job_explains.Count)
            {
                Console.WriteLine($"{this.job_explains[index]}");
                Console.WriteLine($"{this.SelectedJobReson} 바꾸기전 추천이유");
                this.SelectedJobExplain = this.job_explains[index];
                this.SelectedJobReson = this.shareDataService.Jobs_REASON[index];
                Console.WriteLine($"{this.shareDataService.Jobs_REASON[index]}쉐어데이터의 값?");
                Console.WriteLine($"{this.SelectedJobReson} 바꾼후 추천이유");
                this.Wantjob = this.jobs[index].Name;
                this.Wantjobtext = this.wantjob + "<< 플랜잇 만들기 !!";
            }
        }

        [RelayCommand] private async Task Serch_to_sever()
        {
            Console.WriteLine($"검색할 직업! : {this.SerchJob}");
            // 서버에게 요청
            if (!(this.SerchJob == ""))
            {
                JObject result = await this._authService.PlanIT_serch(this.SerchJob);
                Console.WriteLine($"서버가 준값 : {result.ToString()}");
                if (!(result["protocol"]?.ToString() == "100_5_1"))
                {
                    //실패!
                    Console.WriteLine("이상한 직업인가봐요~ 검색 실패!");
                }
                else
                {
                    // 성공하면 직업, 직업설명 추가로 저장
                    Console.WriteLine("직업 검색 성공!");
                    foreach (var job in result["jobs"])
                    {
                        // 이유의 값이 널이아니면 저장
                        if (job["reason"].ToString() != null)
                        {
                            int lastIndex = jobs.Last().Index;
                            this.shareDataService.Jobs_REASON.Add(job["reason"].ToString());
                            this.Job_explains.Add(job["description"].ToString());
                            JobItem item = new JobItem()
                            {
                                Name = job["job"].ToString(),
                                Index = lastIndex
                            };
                            this.jobs.Add(item);
                        }
                    }
                }
                // 검색했으면 초기화
                this.SerchJob = "검색은 여기에서 해주세요!";
            }
            else
            {
                Console.WriteLine("검색할 게없네요");
            }
        }
        [RelayCommand] private async Task Make_PlanIT()
        {
            Console.WriteLine("플랜잇 생성!!");
            Console.WriteLine($"생성할 직업! : {this.Wantjob}");
            // 서버에게 요청
            if (!(this.Wantjob == ""))
            {
                JObject result = await this._authService.PlanIT_make(this.Wantjob, this.shareDataService.User_id);
                Console.WriteLine($"서버가 준값 : {result.ToString()}");
                if (!(result["protocol"]?.ToString() == "100_6_1"))
                {
                    Console.WriteLine("요청 실패!");
                    //실패!
                }
                else
                {
                    Console.WriteLine("만들기 성공!");
                    // 성공! 
                    // 쉐어 데이터 서비스에 서버에게 받은 데이터 저장후 화면전환
                    //this._shareDataService
                    this._navigationStore.CurrentViewModel = this._serviceProvider.GetRequiredService<growth_check_ViewModel>();
                }
                // 검색했으면 초기화
                this.SerchJob = "";
            }
            else
            {
                Console.WriteLine("검색할 게없네요");
            }
        }
        [RelayCommand] private void RectangleClick()
        {
            this.shareDataService.IsDetailVisible = false;
            Console.WriteLine("사각형 클릭됨!");
        }
        [RelayCommand] private void ClearText()
        {
            this.SerchJob = "";
            Console.WriteLine("검색창 클릭됨!");
        }
    }
}

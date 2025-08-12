using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using RunNow.Core;
using RunNow.Services;
using static RunNow.Core.ShareDataService;

namespace RunNow.ViewModels
{
    public partial class growth_check_ViewModel : ObservableObject
    {
        private readonly NavigationStore _navigationStore; // 메인에게 화면넘겨! 객체
        private readonly IAuthService _authService;         // 서버와 통신 객체
        public ShareDataService shareDataService { get; set; }    // 유저 데이터 객체
        private readonly IServiceProvider _serviceProvider;   // 서비스 프로바이더 객체

        public growth_check_ViewModel(NavigationStore navigationStore, IAuthService authService, IServiceProvider serviceProvider, ShareDataService shareDataService)
        {
            // 매개인자로 받은거 복사하기~
            this._serviceProvider = serviceProvider;
            this._authService = authService;
            this._navigationStore = navigationStore;
            this.shareDataService = shareDataService;

            // 생성자아아
            this._currentDate = DateTime.Now;
            this.GenerateCalendar(_currentDate);
            this.UpdateSummary();
            foreach (var goal in this.shareDataService.Goals)
            {
                if(goal.Date != "")
                {
                    this.CompleteGoals.Add(goal);
                }
            }
        }
        public ObservableCollection<DayModel> Days { get; set; } = new ObservableCollection<DayModel>();


        private DateTime _currentDate;
        public string CurrentMonthText => $"{_currentDate:yyyy년 M월}";

        public partial class DayModel : ObservableObject
        {
            [ObservableProperty] private string? dayText; // s날짜
            [ObservableProperty] private DateTime? date;
            [ObservableProperty] private string? tmp;
            private Visibility DayVisibility => string.IsNullOrWhiteSpace(Tmp) ? Visibility.Collapsed : Visibility.Visible;
        }
        // 팝업 관련 프로퍼티 추가
        //[ObservableProperty] private WorkRequestManager? selectedDayData;
        [ObservableProperty] private bool isDetailVisible;
        [ObservableProperty] private string scheduleSummaryText;
        [ObservableProperty] private bool popup = false; // 날짜눌렀을때 팝업
        //[ObservableProperty] private ICollectionView GoalsView;
        [ObservableProperty] private ObservableCollection<GoalDisplay> goalsView; // 카테고리 목표들!
        [ObservableProperty] private string? selectedCategory; // "자격증" / "기술" / "경험" / null(전체)
        public List<string> SelectedGoals = new List<string>(); // 고른 목표들 변수
        [ObservableProperty] private ObservableCollection<GoalDisplay> completeGoals = new ObservableCollection<GoalDisplay>(); // 고른 목표들 변수

        public ICommand RectangleMouseDownCommand { get; }

        private void GenerateCalendar(DateTime targetDate)
        {
            this.Days.Clear();

            DateTime firstDayOfMonth = new DateTime(targetDate.Year, targetDate.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(targetDate.Year, targetDate.Month);
            int skipDays = (int)firstDayOfMonth.DayOfWeek;

            for (int i = 0; i < skipDays; i++)
                this.Days.Add(new DayModel { DayText = "" });

            for (int day = 1; day <= daysInMonth; day++)
            {
                foreach (var Goal in this.shareDataService.Goals)
                {
                    int GoalDate = 0;
                    if (Goal.Date == "")
                    {
                        continue;
                    }
                    else
                    {
                        GoalDate = int.Parse(DateTime.Parse(Goal.Date).ToString("yyyyMMdd"));
                    }
       
                    int NowDate = int.Parse(targetDate.ToString("yyyyMM"));
                    string Date = NowDate.ToString() + day.ToString();

                    if (int.Parse(Date) == GoalDate)
                    {
                        this.Days.Add(new DayModel
                        {
                            DayText = day.ToString(),
                            Date = new DateTime(targetDate.Year, targetDate.Month, day),
                            Tmp = "!!목표달성!!"
                        });
                        break;
                    }
                }                
                this.Days.Add(new DayModel
                {
                    DayText = day.ToString(),
                    Date = new DateTime(targetDate.Year, targetDate.Month, day),
                    Tmp = null
                });
            }
        }
        //private void LoadGoals(JObject json)
        //{
        //    var goalArray = json["GOALS_JSON"]?.ToObject<List<JObject>>();
        //    this.shareDataService.Goals = new ObservableCollection<GoalDisplay>(
        //        goalArray.Select(g => new GoalDisplay
        //        {
        //            Category = g["CATEGORY"]?.ToString(),
        //            Goal = g["GOAL"]?.ToString(),
        //            Status = string.IsNullOrEmpty(g["GOAL_DATE"]?.ToString()) ? "진행중" : "완료"
        //        })
        //    );
        //} 
        [RelayCommand] private void PreviousMonth()
        {
            _currentDate = _currentDate.AddMonths(-1);
            GenerateCalendar(_currentDate);
            OnPropertyChanged(nameof(CurrentMonthText));
        }

        [RelayCommand] private void NextMonth()
        {
            _currentDate = _currentDate.AddMonths(1);
            GenerateCalendar(_currentDate);
            OnPropertyChanged(nameof(CurrentMonthText));
        }
        [RelayCommand] private async Task DayClick(DayModel day)
        {
            if (day.Date is null) return;

            Console.WriteLine($"[Model] {day.Date.Value:yyyy-MM-dd} 클릭됨");
            this.Show_Goals("기술");
            this.Popup = true; // 팝업 보이게!
            //// 테스트 데이터 생성
            //var testModel = new WorkRequestManager(_socket, _session)
            //{



            //    Date = day.Date.Value,
            //    Schedule = new ConfirmedWorkScheModel
            //    {
            //        ShiftType = ShiftType.Day,
            //        StartTime = TimeSpan.Parse("09:00"),
            //        EndTime = TimeSpan.Parse("18:00"),
            //        GroupName = "테스트 근무조"
            //    },
            //    Attendance = new AttendanceModel
            //    {
            //        ClockInTime = DateTime.Parse("2025-08-06 09:03"),
            //        ClockOutTime = DateTime.Parse("2025-08-06 18:01")
            //    },
            //    IsRequested = true,
            //    RequestReason = "개인 사유"
            //};

            //SelectedDayData = testModel;
        }
        private void UpdateSummary()
        {
            this.ScheduleSummaryText = $"    내 미래를 그리는 '{this.shareDataService.Wantjob}'를 향한 도전 캘린더";
        }
        [RelayCommand] private void Calendar() // 달려보여주기;
        {
            this.IsDetailVisible = true;
            //this.IsDetailVisible = true;
        }

        [RelayCommand] private void RectangleClick() // 달력 사라져
        {
            IsDetailVisible = false;
            Console.WriteLine("사각형 클릭됨!");
        }

        [RelayCommand] private void Back()
        {
            // 무슨방법이 올바른가?

            //_navigationStore.CurrentViewModel = App.Services.GetRequiredService<MainViewModel>();

            // 이게 MVVM 패턴에 올바르다!
            this._navigationStore.CurrentViewModel = this._serviceProvider.GetRequiredService<Growth_main_ViewModel>();
        }

        [RelayCommand] private void Make_PlanIT() // 마이플래너로
        {
            this._navigationStore.CurrentViewModel =
                this._serviceProvider.GetRequiredService<Growth_My_ViewModel>();
        }
        [RelayCommand] private void RectanglePupup() // 날짜누른팝업 숨기기
        {
            this.Popup = false;
        }

        [RelayCommand] private void Goal_finish()
        {
            string id = this.shareDataService.User_id;
            int Growth_id = int.Parse(this.shareDataService.Growth_ID);
            // 서버에게 10_0 목표달성 요청하기!
            Console.WriteLine("목표달서엉!");
            this._authService.PlanIT_goal(id, Growth_id, this.SelectedGoals);
            this.Popup = false; // 팝업숨기고 sharedata 값변경
        }
        [RelayCommand] private void Show_Goals(string category)
        {
            // CommandParameter로 직접 view에서 인자로 받기
            Console.WriteLine($"지금 카테고리?! : {category}");
            this.SelectedCategory = category;
            // 진행 중(날짜 == "")
            // ✅ 반드시 컬렉션을 먼저 보장
            GoalsView ??= new ObservableCollection<GoalDisplay>();
            GoalsView.Clear();

            var src = shareDataService.Goals ?? Enumerable.Empty<GoalDisplay>();
            foreach (var g in src.Where(g => string.IsNullOrEmpty(g.Date)
             && string.Equals(g?.Category, category, StringComparison.OrdinalIgnoreCase))
                                 .OrderByDescending(g => g.Importance)
                                 .ThenBy(g => g.Goal))                                 
            {
                GoalsView.Add(g);
            }
        }
        [RelayCommand] private void ToggleSelect(GoalDisplay item)
        {
            Console.WriteLine("토글매서드 실행!");
            if (item is null) return;
            item.IsSelected = !item.IsSelected;
            if (item.IsSelected)
            {
                if(this.SelectedGoals != null)
                {
                    if (!SelectedGoals.Contains(item.Goal)) SelectedGoals.Add(item.Goal);
                    
                }
                else
                {
                    this.SelectedGoals[0] = item.Goal;
                }
                Console.WriteLine($"고른 목표!{item.Goal}");
            }
            else
            {
                SelectedGoals.Remove(item.Goal);
                Console.WriteLine($"고른 목표!{item.Goal}");
            }
        }
    }
}

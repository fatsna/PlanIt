using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using RunNow.Core;
using RunNow.Services;

namespace RunNow.ViewModels
{
    public partial class growth_check_ViewModel : ObservableObject
    {
        private readonly NavigationStore _navigationStore; // 메인에게 화면넘겨! 객체
        private readonly IAuthService _authService;         // 서버와 통신 객체
        private readonly ShareDataService _shareDataService;    // 유저 데이터 객체
        private readonly IServiceProvider _serviceProvider;   // 서비스 프로바이더 객체

        public growth_check_ViewModel(NavigationStore navigationStore, IAuthService authService, IServiceProvider serviceProvider, ShareDataService shareDataService)
        {
            // 매개인자로 받은거 복사하기~
            this._serviceProvider = serviceProvider;
            this._authService = authService;
            this._navigationStore = navigationStore;
            this._shareDataService = shareDataService;

            // 생성자아아
            this._currentDate = DateTime.Now;
            this.GenerateCalendar(_currentDate);
            this.UpdateSummary();
        }
        public ObservableCollection<DayModel> Days { get; set; } = new ObservableCollection<DayModel>();

        private DateTime _currentDate;
        public string CurrentMonthText => $"{_currentDate:yyyy년 M월}";

        public partial class DayModel : ObservableObject
        {
            [ObservableProperty] private string? dayText; // s날짜
            [ObservableProperty] private DateTime? date;
            [ObservableProperty] private string? tmp;
        }
        // 팝업 관련 프로퍼티 추가
        //[ObservableProperty] private WorkRequestManager? selectedDayData;
        [ObservableProperty] private bool isDetailVisible;

        [ObservableProperty] private int workDay = 10;
        [ObservableProperty] private int nightCnt = 8;
        [ObservableProperty] private int offCnt = 6;
        [ObservableProperty] private string scheduleSummaryText;
        public ICommand RectangleMouseDownCommand { get; }

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
                this.Days.Add(new DayModel
                {
                    DayText = day.ToString(),
                    Date = new DateTime(targetDate.Year, targetDate.Month, day),
                    Tmp = "안녕하세요" + day.ToString() + "일!"
                });
            }
        }
        [RelayCommand] private async Task DayClick(DayModel day)
        {
            if (day.Date is null) return;

            Console.WriteLine($"[Model] {day.Date.Value:yyyy-MM-dd} 클릭됨");

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
            this.ScheduleSummaryText = $"     근무일 {this.WorkDay}일 / 야간 {this.NightCnt}일 / 휴무 {this.OffCnt}일";
        }
        [RelayCommand] private void Calendar()
        {
            this.IsDetailVisible = true;
        }

        [RelayCommand] private void RectangleClick()
        {
            this.IsDetailVisible = false;
            Console.WriteLine("사각형 클릭됨!");
        }

        [RelayCommand] private void Back()
        {
            // 무슨방법이 올바른가?

            //_navigationStore.CurrentViewModel = App.Services.GetRequiredService<MainViewModel>();

            // 이게 MVVM 패턴에 올바르다!
            this._navigationStore.CurrentViewModel = this._serviceProvider.GetRequiredService<Growth_main_ViewModel>();
        }
    }
}

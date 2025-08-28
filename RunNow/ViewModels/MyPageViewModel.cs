using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using RunNow.Services;

namespace RunNow.ViewModels
{
    public partial class MyPageViewModel : ObservableObject
    {
        private readonly IAuthService _auth;

        [ObservableProperty] private string? name;     // U_NAME
        [ObservableProperty] private string? userId;   // U_ID
        [ObservableProperty] private string? address;  // U_ADDRESS
        [ObservableProperty] private string? phone;    // U_PHONE

        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private string? error;

        // 단일 결과 표시
        [ObservableProperty] private string? latestResultContent;

        // (선택) 호환용 리스트
        public ObservableCollection<HistoryItem> Histories { get; } = new();

        public string? Uid { get; private set; }

        public MyPageViewModel(IAuthService auth) => _auth = auth;

        public async Task InitializeAsync(string? uid)
        {
            Uid = uid;
            await LoadAsync();
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (string.IsNullOrWhiteSpace(Uid)) { Error = "로그인 정보(U_ID)가 없습니다."; return; }

            IsLoading = true; Error = null;
            try
            {
                var res = await _auth.GetMyPageAsync(Uid);

                // 1) 내정보: user_info 우선
                var profile = res?["user_info"] ?? res?["PROFILE"] ?? res?["내정보"] ?? res;
                UserId = profile?["U_ID"]?.ToString();
                Name = profile?["U_NAME"]?.ToString();
                Address = profile?["U_ADDRESS"]?.ToString();
                Phone = profile?["U_PHONE"]?.ToString();

                // 2) 검사결과: latest_result 1건
                LatestResultContent = null;
                Histories.Clear();

                var latest = res?["latest_result"];
                if (latest is JObject lo)
                {
                    var content = lo.Value<string>("RES_CON")
                               ?? lo.Value<string>("content")
                               ?? lo.ToString(Newtonsoft.Json.Formatting.None);
                    LatestResultContent = content;
                    Histories.Add(new HistoryItem { Uid = lo.Value<string>("U_ID"), Content = content });
                }
                else if (latest?.Type == JTokenType.String)
                {
                    LatestResultContent = latest.ToString();
                    Histories.Add(new HistoryItem { Uid = UserId, Content = LatestResultContent });
                }
                else
                {
                    LatestResultContent = "최근 검사 내역이 없습니다.";
                }
            }
            catch (Exception ex) { Error = ex.Message; }
            finally { IsLoading = false; }
        }
    }

    public class HistoryItem
    {
        public int ResId { get; set; }
        public string? Uid { get; set; }
        public string? Content { get; set; }
    }
}

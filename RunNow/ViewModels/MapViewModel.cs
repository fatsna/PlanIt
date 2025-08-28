using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Threading.Tasks;

namespace RunNow.ViewModels
{
    public partial class MapViewModel : ObservableObject
    {
        [ObservableProperty] private string kakaoApiKey = string.Empty;  // JS 앱 키
        [ObservableProperty] private double centerLat = 37.5665;         // 서울
        [ObservableProperty] private double centerLng = 126.9780;
        [ObservableProperty] private int zoomLevel = 3;

        //카카오맵 띄우기
        public async Task InitializeAsync(WebView2 webView)
        {
            await webView.EnsureCoreWebView2Async();

            var mapFolder = Path.Combine(System.AppContext.BaseDirectory, "Assets", "Map");
            Directory.CreateDirectory(mapFolder);

            const string VirtualHost = "localhost"; // 콘솔에 http/https localhost 둘 다 등록 필수

            // 위치 권한 허용
            webView.CoreWebView2.PermissionRequested += (s, e) =>
            {
                if (e.PermissionKind == CoreWebView2PermissionKind.Geolocation &&
                    e.Uri.StartsWith("https://localhost"))
                {
                    e.State = CoreWebView2PermissionState.Allow;
                    e.Handled = true;
                }
            };

            webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                VirtualHost,
                mapFolder,
                CoreWebView2HostResourceAccessKind.Allow
            );

            var url = $"https://{VirtualHost}/index.html?lat={CenterLat}&lng={CenterLng}&level={ZoomLevel}&key={KakaoApiKey}";
            webView.Source = new System.Uri(url);
        }
    }
}

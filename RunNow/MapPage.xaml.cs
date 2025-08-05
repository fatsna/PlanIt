using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;


namespace ByeCompany.Pages
{
    public partial class MapPage : Page
    {
        public MapPage()
        {
            InitializeComponent();
            Loaded += MapPage_Loaded;
        }

        private async void MapPage_Loaded(object sender, RoutedEventArgs e)
        {
            string htmlPath = @"D:\c샵\PlanIt\RunNow\kakaomap.html";
            string uriPath = $"file:///{htmlPath.Replace("\\", "/")}";

            if (!File.Exists(htmlPath))
            {
                MessageBox.Show("❌ HTML 파일이 없습니다:\n" + htmlPath);
                return;
            }

            await MapViewer.EnsureCoreWebView2Async();

            // 🔥 이벤트 연결은 초기화 이후에 해야 함
            //if (MapViewer.CoreWebView2 != null)
            //{
            //    MapViewer.CoreWebView2.ConsoleMessageReceived += (s, args) =>
            //    {
            //        MessageBox.Show($"[JS 콘솔] {args.Message}");
            //    };
            //}

            MapViewer.Source = new Uri(uriPath);
        }



    }
}

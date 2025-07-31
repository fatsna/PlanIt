using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using ByeCompany.Components; // BottomNav가 있는 네임스페이스

namespace ByeCompany.Pages
{
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            // ✅ BottomNav 인스턴스 생성해서 하단에 붙이기
            var bottomNav = new BottomNav();
            Grid.SetRow(bottomNav, 2);              // Row 2에 넣음
            RootGrid.Children.Add(bottomNav);       // Grid에 추가
        }

        private void EmotionAnalysis_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("감정 분석 페이지로 이동합니다.");
        }

        private void CareerAnalysis_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CareerAnalysisPage());
        }

        private void Map_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("지도 페이지로 이동합니다.");
        }

        private void FinanceAnalysis_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new FinanceAnalysisPage());
        }

        private void DeepDiagnosis_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("정밀 테스트 페이지로 이동합니다.");
        }

        private void Work24_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.work24.go.kr/cm/main.do",
                UseShellExecute = true
            });
        }

        private void QuickTest_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new QuickTestPage());
        }
    }
}

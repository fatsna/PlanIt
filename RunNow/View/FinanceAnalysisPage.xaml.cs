using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ByeCompany.Pages
{
    public partial class FinanceAnalysisPage : Page
    {
        public FinanceAnalysisPage()
        {
            InitializeComponent();
        }

        private void Back_Click(object sender, RoutedEventArgs e)   //뒤로가기 버튼
        {
            NavigationService?.Navigate(new MainPage());
        }

        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(AssetsBox.Text, out decimal assets) &&
                decimal.TryParse(ExpenseBox.Text, out decimal expenses))
            {
                if (expenses == 0)
                {
                    ResultText.Text = "지출이 0원일 수는 없습니다.";
                    return;
                }

                int months = (int)(assets / expenses);
                ResultText.Text = $"예상 생존 가능 기간은 약 {months}개월입니다.";
            }
            else
            {
                ResultText.Text = "숫자를 정확히 입력해주세요.";
            }
        }
    }
}

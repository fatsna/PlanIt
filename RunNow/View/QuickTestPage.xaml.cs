using System.Windows;
using System.Windows.Controls;

namespace ByeCompany.Pages
{
    public partial class QuickTestPage : Page
    {
        public QuickTestPage()
        {
            InitializeComponent();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new MainPage());
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            int score = 0;

            if ((q1.SelectedItem as ComboBoxItem)?.Content.ToString() == "그렇다") score++;
            if ((q2.SelectedItem as ComboBoxItem)?.Content.ToString() == "그렇다") score++;
            if ((q3.SelectedItem as ComboBoxItem)?.Content.ToString() == "그렇다") score++;

            string result;
            if (score == 0) result = "컨디션 아주 좋음!";
            else if (score == 1) result = "약간의 피로가 있습니다.";
            else result = "번아웃이 의심됩니다. 진단을 추천합니다.";

            MessageBox.Show(result, "퀵 테스트 결과");
        }
    }
}

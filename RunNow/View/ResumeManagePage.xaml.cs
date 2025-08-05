using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RunNow.View
{
    public partial class ResumeManagePage : Page
    {
        public ResumeManagePage()
        {
            InitializeComponent();

            // 출생년도 1950 ~ 2010
            for (int year = 1950; year <= 2010; year++)
            {
                YearBox.Items.Add(year.ToString());
            }
            // 월
            for (int month = 1; month <= 12; month++)
            {
                MonthBox.Items.Add(month.ToString("D2"));
            }
            // 일
            for (int day = 1; day <= 31; day++)
            {
                DayBox.Items.Add(day.ToString("D2"));
            }
        }

        private void SaveResumeButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameBox.Text.Trim();

            // 생년월일
            string year = YearBox.SelectedItem?.ToString() ?? "";
            string month = MonthBox.SelectedItem?.ToString() ?? "";
            string day = DayBox.SelectedItem?.ToString() ?? "";

            // 이메일
            string emailId = EmailIdBox.Text.Trim();
            string emailDomain = (EmailDomainBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";

            // 전화번호
            string phonePrefix = (PhonePrefixBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";
            string phoneMid = PhoneMidBox.Text.Trim();
            string phoneEnd = PhoneEndBox.Text.Trim();

            string address = AddressBox.Text.Trim();

            //  입력 다 했는지 검사
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("이름을 입력해주세요.", "입력 확인", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(year) || string.IsNullOrEmpty(month) || string.IsNullOrEmpty(day))
            {
                MessageBox.Show("생년월일을 선택해주세요.", "입력 확인", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(emailId) || string.IsNullOrEmpty(emailDomain))
            {
                MessageBox.Show("이메일을 입력해주세요.", "입력 확인", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(phonePrefix) || string.IsNullOrEmpty(phoneMid) || string.IsNullOrEmpty(phoneEnd))
            {
                MessageBox.Show("전화번호를 모두 입력해주세요.", "입력 확인", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(address))
            {
                MessageBox.Show("주소를 입력해주세요.", "입력 확인", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 생년월일 / 이메일 / 전화번호 조합
            string birthdate = $"{year}-{month}-{day}";
            string email = $"{emailId}@{emailDomain}";
            string phone = $"{phonePrefix}-{phoneMid}-{phoneEnd}";
            string career = CareerBox.Text.Trim();
            string notes = NotesBox.Text.Trim();

            // 확인용 메시지
            string summary = $"이름: {name}\n생년월일: {birthdate}\n이메일: {email}\n전화: {phone}\n주소: {address}\n\n경력사항:\n{career}\n\n특이사항:\n{notes}";
            MessageBox.Show(summary, "입력 내용", MessageBoxButton.OK, MessageBoxImage.Information);

            // TODO: JSON 저장 또는 DB 저장 로직 추가 가능
            //여기 값 들고가서 서버에 전송하기 월욜에 여기 아래에 붙이면 됨.
        }

        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 숫자만 입력되도록 제한
            e.Handled = !e.Text.All(char.IsDigit);
        }

    }

}

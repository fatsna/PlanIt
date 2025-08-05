using PlanIt.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PlanIt.Views
{
    public partial class RegisterPage : Page
    {
        private RegisterViewModel vm;

        public RegisterPage()
        {
            InitializeComponent();
            vm = new RegisterViewModel();
            DataContext = vm;

            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
            ConfirmPasswordBox.PasswordChanged += ConfirmPasswordBox_PasswordChanged;
            YearComboBox.SelectionChanged += (_, __) => ValidateForm();
            MonthComboBox.SelectionChanged += (_, __) => ValidateForm();
            DayComboBox.SelectionChanged += (_, __) => ValidateForm();

            InitBirthDateCombos();
        }

        private async void CheckDuplicate_Click(object sender, RoutedEventArgs e)
        {
            string userId = vm.UserId?.Trim();

            if (string.IsNullOrEmpty(userId))
            {
                MessageBox.Show("아이디를 입력하세요.");
                return;
            }

            bool isDuplicate = await CheckUserIdExistsAsync(userId);

            if (isDuplicate)
            {
                MessageBox.Show("이미 사용 중인 아이디입니다.");
            }
            else
            {
                MessageBox.Show("사용 가능한 아이디입니다!");
                UserIdTextBox.IsEnabled = false;
                ValidateForm();
            }
        }

        private async Task<bool> CheckUserIdExistsAsync(string userId)
        {
            await Task.Delay(500);
            var existingUsers = new[] { "admin", "test", "yesom" };
            return existingUsers.Contains(userId.ToLower());
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            vm.Password = PasswordBox.Password;
            ValidatePasswordMatch();
            ValidateForm();
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ValidatePasswordMatch();
            ValidateForm();
        }

        private void ValidatePasswordMatch()
        {
            if (!string.IsNullOrEmpty(PasswordBox.Password) &&
                !string.IsNullOrEmpty(ConfirmPasswordBox.Password))
            {
                PasswordMismatchText.Visibility = PasswordBox.Password == ConfirmPasswordBox.Password
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
            else
            {
                PasswordMismatchText.Visibility = Visibility.Collapsed;
            }
        }

        private void InitBirthDateCombos()
        {
            for (int year = DateTime.Now.Year; year >= 1900; year--)
                YearComboBox.Items.Add(year);

            for (int month = 1; month <= 12; month++)
                MonthComboBox.Items.Add(month.ToString("D2"));

            for (int day = 1; day <= 31; day++)
                DayComboBox.Items.Add(day.ToString("D2"));
        }

        private void PhoneBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void PhoneBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PhoneBox1.Text.Length == PhoneBox1.MaxLength)
                PhoneBox2.Focus();
            ValidateForm();
        }

        private void PhoneBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PhoneBox2.Text.Length == PhoneBox2.MaxLength)
                PhoneBox3.Focus();
            ValidateForm();
        }

        private void PhoneBox3_TextChanged(object sender, TextChangedEventArgs e)
        {
            vm.PhoneNumber = $"{PhoneBox1.Text}{PhoneBox2.Text}{PhoneBox3.Text}";
            ValidateForm();
        }

        private void FaceRegister_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(vm.UserId))
            {
                MessageBox.Show("아이디를 먼저 입력하세요.");
                return;
            }

            string pythonPath = @"C:\\Users\\YESOM\\PycharmProjects\\PythonProject1\\.venv\\Scripts\\python.exe";
            string scriptPath = @"C:\\Users\\YESOM\\PycharmProjects\\PythonProject1\\regi_face.py";
            string idArg = vm.UserId.Trim();

            if (!File.Exists(pythonPath))
            {
                MessageBox.Show("Python 실행 파일을 찾을 수 없습니다.");
                return;
            }

            if (!File.Exists(scriptPath))
            {
                MessageBox.Show("regi_face.py 파일을 찾을 수 없습니다.");
                return;
            }

            var psi = new ProcessStartInfo
            {
                FileName = pythonPath,
                Arguments = $"\"{scriptPath}\" \"{idArg}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            try
            {
                using (var process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    output = output.Trim();

                    if (output.Contains("SUCCESS"))
                    {
                        MessageBox.Show("😀 얼굴 등록이 완료되었습니다!");
                    }
                    else if (output.Contains("NO_FACE"))
                    {
                        MessageBox.Show("😢 얼굴이 감지되지 않았어요. 다시 시도해주세요.");
                    }
                    else if (output.Contains("CAMERA_ERROR"))
                    {
                        MessageBox.Show("📷 카메라 문제 발생. 장치를 확인하세요.");
                    }
                    else
                    {
                        MessageBox.Show($"⚠️ 얼굴 등록 실패: {output}\n{error}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Python 실행 오류: " + ex.Message);
            }
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (YearComboBox.SelectedItem != null &&
                MonthComboBox.SelectedItem != null &&
                DayComboBox.SelectedItem != null)
            {
                int year = (int)YearComboBox.SelectedItem;
                int month = int.Parse(MonthComboBox.SelectedItem.ToString());
                int day = int.Parse(DayComboBox.SelectedItem.ToString());

                vm.BirthDate = new DateTime(year, month, day);
            }

            MessageBox.Show("회원가입 완료 (예시)");

            var loginpage = new LoginPage();
            NavigationService?.Navigate(loginpage);
        }

        private void ValidateForm()
        {
            bool isValid =
                !string.IsNullOrWhiteSpace(vm.Name) &&
                !string.IsNullOrWhiteSpace(vm.UserId) &&
                !string.IsNullOrWhiteSpace(vm.Password) &&
                !string.IsNullOrWhiteSpace(ConfirmPasswordBox.Password) &&
                (vm.Password == ConfirmPasswordBox.Password) &&
                YearComboBox.SelectedItem != null &&
                MonthComboBox.SelectedItem != null &&
                DayComboBox.SelectedItem != null &&
                (vm.IsMale || vm.IsFemale) &&
                !string.IsNullOrWhiteSpace(vm.Address) &&
                !string.IsNullOrWhiteSpace(PhoneBox1.Text) &&
                !string.IsNullOrWhiteSpace(PhoneBox2.Text) &&
                !string.IsNullOrWhiteSpace(PhoneBox3.Text) &&
                UserIdTextBox.IsEnabled == false;

            SubmitButton.IsEnabled = isValid;
        }
    }
}

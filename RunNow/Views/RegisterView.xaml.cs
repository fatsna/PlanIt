using RunNow.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace RunNow.Views
{
    public partial class RegisterView : UserControl
    {
        public RegisterView()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel vm)
            {
                vm.SetPassword(((PasswordBox)sender).Password);
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel vm)
            {
                vm.SetConfirmPassword(((PasswordBox)sender).Password);
            }
        }
    }
}

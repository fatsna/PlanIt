using System.Windows;
using RunNow.Views;
using RunNow.ViewModels;

namespace RunNow.Services
{
    public class DialogService : IDialogService
    {
        public void ShowMessage(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public bool ShowConfirmation(string message, string title)
        {
            return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public string ShowCertificateDialog()
        {
            var dialog = new CertificatePopup();
            var vm = new CertificatePopupViewModel();

            string selectedName = null;
            vm.CloseAction = (result) =>
            {
                selectedName = result;
                dialog.DialogResult = true;
                dialog.Close();
            };

            dialog.DataContext = vm;

            return dialog.ShowDialog() == true ? selectedName : null;
        }
    }
}

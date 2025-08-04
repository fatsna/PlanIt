using System.Windows;
using RunNow.Views;

namespace RunNow.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new ChatBotView());
        }
    }
}

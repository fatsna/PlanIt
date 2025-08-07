using Microsoft.Extensions.DependencyInjection;
using RunNow.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Specialized;

namespace RunNow.Views
{
    public partial class ChatBotView : UserControl
    {
        public ChatBotView()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<ChatBotViewModel>();

            Loaded += ChatBotView_Loaded;
        }

        private void ChatBotView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ChatBotViewModel vm)
            {
                vm.Messages.CollectionChanged += Messages_CollectionChanged;
            }
        }

        private void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (ChatListBox.Items.Count > 0)
                {
                    ChatListBox.ScrollIntoView(ChatListBox.Items[^1]); // 마지막 항목으로 스크롤
                }
            }
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            
        }

    }
}

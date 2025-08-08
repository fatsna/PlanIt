using System.Windows.Controls;
using RunNow.ViewModels;

namespace RunNow.Views
{
    public partial class MapView : UserControl
    {
        public MapView()
        {
            InitializeComponent();
            Loaded += async (_, __) =>
            {
                if (DataContext is MapViewModel vm)
                    await vm.InitializeAsync(Browser);
            };
        }
    }
}

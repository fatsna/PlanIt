using ByeCompany.Pages;
using System;
using RunNow.View;
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

namespace ByeCompany.Components
{
    public partial class BottomNav : UserControl
    {
        public BottomNav()
        {
            InitializeComponent();
        }

        private void HomeButton_Click(object sender, MouseButtonEventArgs e)
        {
            // NavigationService는 Page에서만 직접 접근 가능 → 부모에서 NavigationService 사용
            var parent = Window.GetWindow(this);
            if (parent is NavigationWindow navWindow)
            {
                navWindow.Navigate(new MainPage());
            }
            else
            {
                // Frame 내에서 동작할 경우
                var frame = FindParent<Frame>(this);
                if (frame != null)
                {
                    frame.Navigate(new MainPage());
                }
            }
        }

        private T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject? parent = VisualTreeHelper.GetParent(child);

            while (parent != null && parent is not T)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as T;
        }
        private void ResumeManage_Click(object sender, MouseButtonEventArgs e)
        {
            // MainPage와 동일한 구조를 재사용
            var parent = Window.GetWindow(this);
            if (parent is NavigationWindow navWindow)
            {
                navWindow.Navigate(new ResumeManagePage());
            }
            else
            {
                var frame = FindParent<Frame>(this);
                if (frame != null)
                {
                    frame.Navigate(new ResumeManagePage());
                }
            }
        }


    }
}
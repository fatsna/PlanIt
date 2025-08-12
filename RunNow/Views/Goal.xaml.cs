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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RunNow.Views
{
    /// <summary>
    /// Goal.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Goal : UserControl
    {
        public event EventHandler Closed;
        private int clickCount = 0;

        public Goal()
        {
            InitializeComponent();
        }

        private void ConfettiCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPosition = e.GetPosition(ConfettiCanvas);
            GenerateConfetti(clickPosition);
            clickCount++;

            if (clickCount >= 5) // 3번 클릭 후 최종 메시지 및 버튼 표시
            {
                MessageBlock.Text = "정말 대단해요! 다음 도전을 시작해볼까요?";
                CloseButton.Visibility = Visibility.Visible;
            }
        }

        private void GenerateConfetti(Point position)
        {
            Random random = new Random();
            int numberOfParticles = 30;

            for (int i = 0; i < numberOfParticles; i++)
            {
                // 색종이 모양 생성
                Rectangle confetti = new Rectangle
                {
                    Width = random.Next(5, 15),
                    Height = random.Next(5, 15),
                    Fill = new SolidColorBrush(GetRandomColor(random)),
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    RenderTransform = new RotateTransform(random.Next(0, 360))
                };

                ConfettiCanvas.Children.Add(confetti);
                Canvas.SetLeft(confetti, position.X);
                Canvas.SetTop(confetti, position.Y);

                // 애니메이션 Storyboard 생성
                Storyboard storyboard = new Storyboard();

                // 이동 애니메이션 (위치, Duration, Easing)
                DoubleAnimation xAnimation = new DoubleAnimation
                {
                    To = position.X + random.Next(-200, 200),
                    Duration = TimeSpan.FromSeconds(2),
                    EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut }
                };
                Storyboard.SetTarget(xAnimation, confetti);
                Storyboard.SetTargetProperty(xAnimation, new PropertyPath("(Canvas.Left)"));

                DoubleAnimation yAnimation = new DoubleAnimation
                {
                    To = position.Y - random.Next(200, 400),
                    Duration = TimeSpan.FromSeconds(2),
                    EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut }
                };
                Storyboard.SetTarget(yAnimation, confetti);
                Storyboard.SetTargetProperty(yAnimation, new PropertyPath("(Canvas.Top)"));

                // 불투명도 애니메이션
                DoubleAnimation opacityAnimation = new DoubleAnimation
                {
                    To = 0,
                    BeginTime = TimeSpan.FromSeconds(1),
                    Duration = TimeSpan.FromSeconds(1)
                };
                Storyboard.SetTarget(opacityAnimation, confetti);
                Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));

                storyboard.Children.Add(xAnimation);
                storyboard.Children.Add(yAnimation);
                storyboard.Children.Add(opacityAnimation);

                storyboard.Completed += (s, ev) => ConfettiCanvas.Children.Remove(confetti);
                storyboard.Begin();
            }
        }

        private Color GetRandomColor(Random random)
        {
            byte[] colorBytes = new byte[3];
            random.NextBytes(colorBytes);
            return Color.FromRgb(colorBytes[0], colorBytes[1], colorBytes[2]);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }
    }
}
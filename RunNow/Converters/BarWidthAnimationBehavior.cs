using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace RunNow.Converters
{
    public static class BarWidthAnimationBehavior
    {
        // 1. 의존성 속성 정의
        public static readonly DependencyProperty AnimateToProperty =
            DependencyProperty.RegisterAttached(
                "AnimateTo",
                typeof(double),
                typeof(BarWidthAnimationBehavior),
                new PropertyMetadata(0.0, OnAnimateToChanged));

        // 2. Getter (필수)
        public static double GetAnimateTo(DependencyObject obj)
        {
            return (double)obj.GetValue(AnimateToProperty);
        }

        // 3. Setter (필수)
        public static void SetAnimateTo(DependencyObject obj, double value)
        {
            obj.SetValue(AnimateToProperty, value);
        }

        // 4. 값 변경 시 애니메이션 실행
        private static void OnAnimateToChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Rectangle rect && e.NewValue is double targetWidth)
            {
                var animation = new DoubleAnimation
                {
                    From = 0,
                    To = targetWidth,
                    Duration = TimeSpan.FromSeconds(0.6),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                rect.BeginAnimation(FrameworkElement.WidthProperty, animation);
            }
        }
    }
}
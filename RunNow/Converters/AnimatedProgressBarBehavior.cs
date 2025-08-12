using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace RunNow.Converters
{
    public static class AnimatedProgressBarBehavior
    {
        public static readonly DependencyProperty EnableAnimationProperty =
            DependencyProperty.RegisterAttached("EnableAnimation", typeof(bool), typeof(AnimatedProgressBarBehavior),
                new PropertyMetadata(false, OnEnableChanged));

        public static void SetEnableAnimation(DependencyObject d, bool v) => d.SetValue(EnableAnimationProperty, v);
        public static bool GetEnableAnimation(DependencyObject d) => (bool)d.GetValue(EnableAnimationProperty);

        static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ProgressBar bar) return;
            if ((bool)e.NewValue)
            {
                DependencyPropertyDescriptor.FromProperty(ProgressBar.ValueProperty, typeof(ProgressBar))
                    .AddValueChanged(bar, (_, __) =>
                    {
                        double from = bar.Tag as double? ?? 0.0;
                        double to = bar.Value;
                        var anim = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(600))
                        {
                            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                        };
                        bar.BeginAnimation(ProgressBar.ValueProperty, anim, HandoffBehavior.SnapshotAndReplace);
                        bar.Tag = to;
                    });
            }
        }
    }
}

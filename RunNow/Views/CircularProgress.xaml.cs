using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace RunNow.Views
{
    public partial class CircularProgress : UserControl
    {
        public CircularProgress()
        {
            InitializeComponent();
            SizeChanged += (_, __) => Redraw();
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(CircularProgress),
                new PropertyMetadata(0.0, OnValueChanged));
        public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }

        public static readonly DependencyProperty AnimatedValueProperty =
            DependencyProperty.Register(nameof(AnimatedValue), typeof(double), typeof(CircularProgress),
                new PropertyMetadata(0.0, (s, e) => ((CircularProgress)s).Redraw()));
        public double AnimatedValue { get => (double)GetValue(AnimatedValueProperty); set => SetValue(AnimatedValueProperty, value); }

        public string PercentText => $"{Math.Round(AnimatedValue * 100)}%";

        static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (CircularProgress)d;
            double from = self.AnimatedValue;
            double to = Math.Max(0, Math.Min(1, (double)e.NewValue));

            var anim = new DoubleAnimation(from, to, new Duration(TimeSpan.FromMilliseconds(700)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            self.BeginAnimation(AnimatedValueProperty, anim, HandoffBehavior.SnapshotAndReplace);
        }

        void Redraw()
        {
            var v = Math.Max(0, Math.Min(1, AnimatedValue));
            double w = ActualWidth, h = ActualHeight;
            double r = Math.Min(w, h) / 2 - 8;
            Point center = new(w / 2, h / 2);

            if (v <= 0) { ArcPath.Data = null; return; }

            double start = -90 * Math.PI / 180.0;
            double sweep = 360 * v * Math.PI / 180.0;

            Point p0 = new(center.X + r * Math.Cos(start), center.Y + r * Math.Sin(start));
            Point p1 = new(center.X + r * Math.Cos(start + sweep), center.Y + r * Math.Sin(start + sweep));

            var fig = new PathFigure { StartPoint = p0, IsClosed = false };
            fig.Segments.Add(new ArcSegment
            {
                Point = p1,
                Size = new Size(r, r),
                IsLargeArc = v > 0.5,
                SweepDirection = SweepDirection.Clockwise
            });
            ArcPath.Data = new PathGeometry(new[] { fig });
        }
    }
}

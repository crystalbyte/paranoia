using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace Crystalbyte.Paranoia.Behaviors {
    /// <summary>
    /// This behavior enables scrolling a surface by using touch gestures or mouse drag.
    /// Source is taken from http://matthamilton.net/touchscrolling-for-scrollviewer and altered to support horizontal scrolling.
    /// </summary>
    public sealed class ScrollByTouchBehavior : DependencyObject {

        public static double HorizontalFriction = 3.0d;
        public static double VerticalFriction = 3.0d;
        public static double DeadZone = 8.0d;

        private static readonly Dictionary<object, MouseCapture> Captures = new Dictionary<object, MouseCapture>();
        private static readonly DispatcherTimer Timer = new DispatcherTimer(
            TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, OnTimerElapsed, Dispatcher.CurrentDispatcher);

        public static bool GetIsEnabled(DependencyObject obj) {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value) {
            obj.SetValue(IsEnabledProperty, value);
        }

        public bool IsEnabled {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(ScrollByTouchBehavior), new UIPropertyMetadata(false, IsEnabledChanged));

        private static void IsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var target = d as ScrollViewer;
            if (target == null)
                return;

            if ((bool)e.NewValue) {
                target.Loaded += OnTargetLoaded;
            } else {
                OnTargetUnloaded(target, new RoutedEventArgs());
            }
        }

        static void OnTargetUnloaded(object sender, RoutedEventArgs e) {
            Debug.WriteLine("Target Unloaded");

            var target = sender as ScrollViewer;
            if (target == null)
                return;

            Captures.Remove(sender);

            target.Loaded -= OnTargetLoaded;
            target.Unloaded -= OnTargetUnloaded;
            target.PreviewMouseLeftButtonDown -= OnTargetPreviewMouseLeftButtonDown;
            target.PreviewMouseMove -= OnTargetPreviewMouseMove;
            target.PreviewMouseLeftButtonUp -= OnTargetPreviewMouseLeftButtonUp;
        }

        static void OnTargetPreviewMouseLeftButtonDown(object sender, MouseEventArgs e) {
            var target = sender as ScrollViewer;
            if (target == null)
                return;

            Captures[sender] = new MouseCapture {
                IsReleased = true,
                HorizontalOffset = target.HorizontalOffset,
                VerticalOffset = target.VerticalOffset,
                Velocity = new Point(),
                Point = e.GetPosition(target),
                LastPoint = e.GetPosition(target)
            };
        }

        private static void OnTimerElapsed(object sender, EventArgs e) {
            //Debug.WriteLine("Called OnTimerElapsed.");
            ApplyFrictionToInertia();
        }

        private static void ApplyFrictionToInertia() {
            var pairs = Captures.Where(x => x.Value.IsReleased).ToArray();
            foreach (var pair in pairs) {
                var target = (ScrollViewer)pair.Key;
                var capture = pair.Value;

                if (Math.Abs(capture.Velocity.X) < DeadZone) {
                    capture.Velocity = new Point(0, capture.Velocity.Y);
                }

                if (Math.Abs(capture.Velocity.Y) < DeadZone) {
                    capture.Velocity = new Point(capture.Velocity.X, 0);
                }

                if (Math.Abs(capture.Velocity.X) < double.Epsilon
                    && Math.Abs(capture.Velocity.Y) < double.Epsilon) {
                    continue;
                }

                // Apply friction.
                capture.Velocity = new Point(
                    capture.Velocity.X > 0
                    ? capture.Velocity.X - HorizontalFriction
                    : capture.Velocity.X + HorizontalFriction,
                    capture.Velocity.Y > 0
                    ? capture.Velocity.Y - VerticalFriction
                    : capture.Velocity.Y + VerticalFriction);

                var vx = capture.LastPoint.X + capture.Velocity.X;
                var vy = capture.LastPoint.Y + capture.Velocity.Y;

                capture.LastPoint = new Point(vx, vy);

                Debug.WriteLine("Scrolling by {0} x {1}.", vx, vy);
                target.ScrollToVerticalOffset(capture.VerticalOffset - vy);
                target.ScrollToHorizontalOffset(capture.HorizontalOffset - vx);
            }
        }

        static void OnTargetLoaded(object sender, RoutedEventArgs e) {
            var target = sender as ScrollViewer;
            if (target == null)
                return;

            Debug.WriteLine("Target Loaded");

            target.Unloaded += OnTargetUnloaded;
            target.PreviewMouseLeftButtonDown += OnTargetPreviewMouseLeftButtonDown;
            target.PreviewMouseMove += OnTargetPreviewMouseMove;
            target.PreviewMouseLeftButtonUp += OnTargetPreviewMouseLeftButtonUp;
        }

        static void OnTargetPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var target = sender as ScrollViewer;
            if (target == null)
                return;

            target.ReleaseMouseCapture();

            if (!Captures.ContainsKey(sender)) {
                return;
            }

            var capture = Captures[sender];
            capture.IsReleased = true;
            capture.LastPoint = Mouse.GetPosition(target);
        }

        static void OnTargetPreviewMouseMove(object sender, MouseEventArgs e) {
            if (!Captures.ContainsKey(sender))
                return;

            var target = sender as ScrollViewer;
            if (target == null)
                return;

            if (Mouse.LeftButton != MouseButtonState.Pressed) {
                return;
            }

            if (Mouse.DirectlyOver is Thumb) {
                return;
            }

            var point = e.GetPosition(target);

            var capture = Captures[sender];
            capture.Velocity = new Point(point.X - capture.LastPoint.X,
                point.Y - capture.LastPoint.Y);

            Debug.WriteLine("Velocity now @ {0}.", capture.Velocity.X);

            capture.LastPoint = point;

            var dx = point.X - capture.Point.X;
            var dy = point.Y - capture.Point.Y;
            if (Math.Abs(dy) > 5 || Math.Abs(dx) > 10) {
                capture.IsReleased = false;
                target.CaptureMouse();
            }

            target.ScrollToVerticalOffset(capture.VerticalOffset - dy);
            target.ScrollToHorizontalOffset(capture.HorizontalOffset - dx);

        }

        internal class MouseCapture {
            public bool IsReleased { get; set; }
            public double HorizontalOffset { get; set; }
            public double VerticalOffset { get; set; }
            public Point Point { get; set; }
            public Point Velocity { get; set; }
            public Point LastPoint { get; set; }
        }
    }
}
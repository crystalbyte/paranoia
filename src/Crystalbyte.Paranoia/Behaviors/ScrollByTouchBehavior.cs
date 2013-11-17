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
                HorizontalOffset = target.HorizontalOffset,
                VerticalOffset = target.VerticalOffset,
                Point = e.GetPosition(target),
            };
        }

        private static void OnTimerElapsed(object sender, EventArgs e) {
            //Debug.WriteLine("Called OnTimerElapsed.");
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
        }

        static void OnTargetPreviewMouseMove(object sender, MouseEventArgs e) {
            if (!Captures.ContainsKey(sender))
                return;

            var target = sender as ScrollViewer;
            if (target == null)
                return;

            if (Mouse.LeftButton != MouseButtonState.Pressed) {
                Captures.Remove(sender);
                return;
            }

            if (Mouse.DirectlyOver is Thumb) {
                return;
            }

            var point = e.GetPosition(target);

            var capture = Captures[sender];

            var dx = point.X - capture.Point.X;
            var dy = point.Y - capture.Point.Y;
            if (Math.Abs(dy) > 5 || Math.Abs(dx) > 10) {
                target.CaptureMouse();
            }

            target.ScrollToVerticalOffset(capture.VerticalOffset - dy);
            target.ScrollToHorizontalOffset(capture.HorizontalOffset - dx);

        }

        internal class MouseCapture {
            public double HorizontalOffset { get; set; }
            public double VerticalOffset { get; set; }
            public Point Point { get; set; }
        }
    }
}
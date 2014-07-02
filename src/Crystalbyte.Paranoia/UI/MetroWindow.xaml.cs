#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// This class represents a window with an integrated ribbon.
    /// </summary>
    public class MetroWindow : Window {

        #region Private Fields

        private HwndSource _hwndSource;

        #endregion

        #region Construction

        static MetroWindow() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MetroWindow),
                new FrameworkPropertyMetadata(typeof(MetroWindow)));
        }

        public MetroWindow() {
            Loaded += OnWindowLoaded;
            SourceInitialized += OnSourceInitialized;

            CommandBindings.Add(new CommandBinding(WindowCommands.Maximize, OnMaximize));
            CommandBindings.Add(new CommandBinding(WindowCommands.Minimize, OnMinimize));
            CommandBindings.Add(new CommandBinding(WindowCommands.RestoreDown, OnRestoredDown));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnClose));
        }

        #endregion

        #region Dependency Properties

        public Thickness FramePadding {
            get { return (Thickness)GetValue(FramePaddingProperty); }
            set { SetValue(FramePaddingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FramePadding.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FramePaddingProperty =
            DependencyProperty.Register("FramePadding", typeof(Thickness), typeof(MetroWindow), new PropertyMetadata(new Thickness(0)));

        public bool IsNormalized {
            get { return (bool)GetValue(IsNormalizedProperty); }
            set { SetValue(IsNormalizedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for QuickAccessCommands.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsNormalizedProperty =
            DependencyProperty.Register("IsNormalized", typeof(bool), typeof(MetroWindow),
                new PropertyMetadata(false));

        public bool IsMaximized {
            get { return (bool)GetValue(IsMaximizedProperty); }
            set { SetValue(IsMaximizedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMaximized.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMaximizedProperty =
            DependencyProperty.Register("IsMaximized", typeof(bool), typeof(MetroWindow), new PropertyMetadata(false));

        public Brush AccentBrush {
            get { return (Brush)GetValue(AccentBrushProperty); }
            set { SetValue(AccentBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AccentBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AccentBrushProperty =
            DependencyProperty.Register("AccentBrush", typeof(Brush), typeof(MetroWindow), new PropertyMetadata(null));

        public Brush HoverBrush {
            get { return (Brush)GetValue(HoverBrushProperty); }
            set { SetValue(HoverBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HoverBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HoverBrushProperty =
            DependencyProperty.Register("HoverBrush", typeof(Brush), typeof(MetroWindow), new PropertyMetadata(null));

        #endregion

        #region Event Handlers

        private void OnWindowLoaded(object sender, RoutedEventArgs e) {
            UpdateWindowStates();
        }

        private void OnMinimize(object sender, ExecutedRoutedEventArgs e) {
            WindowState = WindowState.Minimized;
            e.Handled = true;
        }

        private void OnMaximize(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Maximized;
            e.Handled = true;
        }

        private void OnClose(object sender, RoutedEventArgs e) {
            Close();
            e.Handled = true;
        }

        private void OnRestoredDown(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Normal;
            e.Handled = true;
        }

        private void OnSourceInitialized(object sender, EventArgs e) {
            var helper = new WindowInteropHelper(this);
            _hwndSource = HwndSource.FromHwnd(helper.Handle);
        }

        #endregion

        #region Class Overrides

        protected override void OnStateChanged(EventArgs e) {
            base.OnStateChanged(e);
            UpdateWindowStates();
            UpdateWindowBounds();
        }

        #endregion

        #region Methods


        private void UpdateWindowStates() {
            IsNormalized = WindowState == WindowState.Normal;
            IsMaximized = WindowState == WindowState.Maximized;
        }

        #endregion

        #region Native Window Support

        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedMember.Local
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable MemberCanBePrivate.Local

        const int MONITOR_DEFAULTTONEAREST = 2;
        const int WINDOWPOSCHANGING = 0x0046;

        private void UpdateWindowBounds() {
            if (WindowState == WindowState.Normal) {
                BorderThickness = new Thickness(1);
                FramePadding = new Thickness(0);
                return;
            }

            var monitor = SafeNativeMethods.MonitorFromWindow(_hwndSource.Handle, MONITOR_DEFAULTTONEAREST);
            var info = new MONITORINFOEX { cbSize = Marshal.SizeOf(typeof(MONITORINFOEX)) };
            SafeNativeMethods.GetMonitorInfo(new HandleRef(this, monitor), ref info);

            if (_hwndSource.CompositionTarget == null) {
                throw new NullReferenceException("_hwndSource.CompositionTarget == null");
            }

            // All points queried from the Win32 API are not DPI aware.
            // Since WPF is DPI aware, one WPF pixel does not necessarily correspond to a device pixel.
            // In order to convert device pixels (Win32 API) into screen independent pixels (WPF), 
            // the following transformation must be applied to points queried using the Win32 API.
            var matrix = _hwndSource.CompositionTarget.TransformFromDevice;

            // Not DPI aware
            var workingArea = info.rcWork;
            var monitorRect = info.rcMonitor;

            // DPI aware
            var bounds = matrix.Transform(new Point(workingArea.right - workingArea.left,
                    workingArea.bottom - workingArea.top));

            // DPI aware
            var origin = matrix.Transform(new Point(workingArea.left, workingArea.top))
                - matrix.Transform(new Point(monitorRect.left, monitorRect.top));

            // Calulates the offset required to adjust the anchor position for the missing client frame border.
            // An additional -1 must be added to the top to perfectly fit the screen, reason is of yet unknown.
            var left = SystemParameters.WindowNonClientFrameThickness.Left
                + SystemParameters.ResizeFrameVerticalBorderWidth + origin.X;
            var top = SystemParameters.WindowNonClientFrameThickness.Top
                + SystemParameters.ResizeFrameHorizontalBorderHeight
                - SystemParameters.CaptionHeight + origin.Y - 1;

            FramePadding = new Thickness(left, top, 0, 0);
            MaxWidth = bounds.X + SystemParameters.ResizeFrameVerticalBorderWidth + SystemParameters.WindowNonClientFrameThickness.Right;
            MaxHeight = bounds.Y + SystemParameters.ResizeFrameHorizontalBorderHeight + SystemParameters.WindowNonClientFrameThickness.Bottom;
            BorderThickness = new Thickness(0);
        }

        [SuppressUnmanagedCodeSecurity]
        private static class SafeNativeMethods {
            // To get a handle to the specified monitor
            [DllImport("user32.dll")]
            public static extern IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);

            [DllImport("user32.dll")]
            public static extern bool GetMonitorInfo(HandleRef hmonitor, ref MONITORINFOEX monitorInfo);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFOEX {
            public int cbSize;
            public RECT rcMonitor; // Total area
            public RECT rcWork; // Working area
            public int dwFlags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
            public char[] szDevice;
        }

        // ReSharper restore InconsistentNaming
        // ReSharper restore UnusedMember.Local
        // ReSharper restore FieldCanBeMadeReadOnly.Local
        // ReSharper restore MemberCanBePrivate.Local

        #endregion
    }
}
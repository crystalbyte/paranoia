#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     This class represents a window with an integrated ribbon.
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
            SourceInitialized += OnSourceInitialized;
            Loaded += OnLoaded;

            CommandBindings.Add(new CommandBinding(WindowCommands.Maximize, OnMaximize));
            CommandBindings.Add(new CommandBinding(WindowCommands.Minimize, OnMinimize));
            CommandBindings.Add(new CommandBinding(WindowCommands.RestoreDown, OnRestoreDown));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnClose));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Help, OnHelp));
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            UpdateWindowStates();
        }

        #endregion

        #region Dependency Properties



        public Thickness ActualFramePadding {
            get { return (Thickness)GetValue(ActualFramePaddingProperty); }
            set { SetValue(ActualFramePaddingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActualFramePadding.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActualFramePaddingProperty =
            DependencyProperty.Register("ActualFramePadding", typeof(Thickness), typeof(MetroWindow), new PropertyMetadata(new Thickness(0)));


        public Thickness FramePadding {
            get { return (Thickness)GetValue(FramePaddingProperty); }
            set { SetValue(FramePaddingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FramePadding.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FramePaddingProperty =
            DependencyProperty.Register("FramePadding", typeof(Thickness), typeof(MetroWindow),
                new PropertyMetadata(new Thickness(0), OnFramePaddingChanged));

        private static void OnFramePaddingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var window = (MetroWindow)d;
            window.InitFramePadding((Thickness)e.NewValue);
        }

        private void InitFramePadding(Thickness thickness) {
            ActualFramePadding = thickness;
        }

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

        public string SearchText {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SearchText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register("SearchText", typeof(string), typeof(MetroWindow),
                new PropertyMetadata(string.Empty));

        #endregion

        #region Event Handlers

        private void OnHelp(object sender, ExecutedRoutedEventArgs e) {
            MessageBox.Show("Not yet implemented.");
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

        private void OnRestoreDown(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Normal;
            e.Handled = true;
        }

        private void OnSourceInitialized(object sender, EventArgs e) {
            var helper = new WindowInteropHelper(this);
            _hwndSource = HwndSource.FromHwnd(helper.Handle);
            if (_hwndSource != null)
                _hwndSource.AddHook(WindowProc);

        }

        #endregion

        #region Class Overrides

        protected override void OnStateChanged(EventArgs e) {
            base.OnStateChanged(e);

            UpdateWindowStates();
            ActualFramePadding = IsMaximized ? new Thickness(0) : FramePadding;
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

        private const int MONITOR_DEFAULTTONEAREST = 0x00000002;
        private const int WM_WINDOWPOSCHANGING = 0x0046;
        private const int WM_GETMINMAXINFO = 0x0024;
        private const int SWP_NOMOVE = 0x0002;

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            switch (msg) {
                case WM_GETMINMAXINFO:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
                case WM_WINDOWPOSCHANGING:
                    handled = WmPositionChanging(lParam);
                    break;

            }

            return IntPtr.Zero;
        }

        private bool WmPositionChanging(IntPtr lParam) {
            var target = _hwndSource.CompositionTarget;
            if (target == null) {
                return false;
            }

            var matrix = target.TransformToDevice;
            var minWidth = Convert.ToInt32(matrix.Transform(new Point(MinWidth, 0)).X);
            var minHeight = Convert.ToInt32(matrix.Transform(new Point(0, MinHeight)).Y);

            var pos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));
            if ((pos.flags & SWP_NOMOVE) != 0) {
                return false;
            }

            var isAdjusted = false;
            if (pos.cx < minWidth) {
                pos.cx = minWidth; isAdjusted = true;
            }
            if (pos.cy < minHeight) {
                pos.cy = minHeight; isAdjusted = true;
            }

            if (!isAdjusted) {
                return false;
            }

            Marshal.StructureToPtr(pos, lParam, true);
            return true;
        }

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam) {
            var mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            // Adjust the maximized size and position to fit the work area of the correct monitor

            var monitor = NativeMethods.MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero) {
                var monitorInfo = new MONITORINFOEX {
                    cbSize = Marshal.SizeOf(typeof(MONITORINFOEX))
                };

                NativeMethods.GetMonitorInfo(monitor, ref monitorInfo);
                var rcWorkArea = monitorInfo.rcWork;
                var rcMonitorArea = monitorInfo.rcMonitor;

                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        private static class NativeMethods {
            // To get a handle to the specified monitor
            [DllImport("user32.dll")]
            public static extern IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);

            [DllImport("user32.dll")]
            public static extern bool GetMonitorInfo(IntPtr hmonitor, ref MONITORINFOEX monitorInfo);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPOS {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT {
            public int x;
            public int y;

            public POINT(int x, int y) {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MINMAXINFO {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFOEX {
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
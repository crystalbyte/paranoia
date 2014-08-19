using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for ShadowCaster.xaml
    /// </summary>
    public partial class ShadowCaster {

        #region Private Fields

        private HwndSource _hwndSource;
        private Matrix _matrix;

        #endregion

        public ShadowCaster() {
            InitializeComponent();
            SourceInitialized += OnSourceInitialized;
        }

        public void UpdatePosition(Window window) {
            switch (DockPosition) {
                case Dock.Left:
                    DockLeft(window);
                    break;
                case Dock.Right:
                    DockRight(window);
                    break;
                case Dock.Top:
                    DockTop(window);
                    break;
                case Dock.Bottom:
                    DockBottom(window);
                    break;
            }
        }

        private double Transform(double size) {
            return _matrix.Transform(new Point(size, size)).X;
        }

        private void DockLeft(Window window) {
            Caster.Width = 0.4;
            Caster.HorizontalAlignment = HorizontalAlignment.Right;
            Caster.VerticalAlignment = VerticalAlignment.Stretch;
            Width = 10;
            Left = window.Left - Transform(Width);
            Top = window.Top;
            Height = window.Height;
            DropShadowEffect.Direction = 180;
        }

        private void DockRight(Window window) {
            Caster.Width = 0.4;
            Caster.HorizontalAlignment = HorizontalAlignment.Left;
            Caster.VerticalAlignment = VerticalAlignment.Stretch;
            Width = 10;
            Left = window.Left + window.Width;
            Top = window.Top;
            Height = window.Height;
            DropShadowEffect.Direction = 0;
        }

        private void DockTop(Window window) {
            Caster.Height = 0.4;
            Caster.HorizontalAlignment = HorizontalAlignment.Stretch;
            Caster.VerticalAlignment = VerticalAlignment.Bottom;
            Height = 10;
            Left = window.Left;
            Top = window.Top - Transform(Height);
            Width = window.Width;
            DropShadowEffect.Direction = 90;
        }

        private void DockBottom(Window window) {
            Caster.Height = 0.4;
            Caster.HorizontalAlignment = HorizontalAlignment.Stretch;
            Caster.VerticalAlignment = VerticalAlignment.Top;
            Height = 10;
            Left = window.Left;
            Top = window.Top + window.Height;
            Width = window.Width;
            DropShadowEffect.Direction = 270;
        }

        private void OnSourceInitialized(object sender, EventArgs e) {
            var helper = new WindowInteropHelper(this);
            _hwndSource = HwndSource.FromHwnd(helper.Handle);
            if (_hwndSource == null) {
                throw new NullReferenceException("_hwndSource");
            }

            // All points queried from the Win32 API are not DPI aware.
            // Since WPF is DPI aware, one WPF pixel does not necessarily correspond to a device pixel.
            // In order to convert device pixels (Win32 API) into screen independent pixels (WPF), 
            // the following transformation must be applied to points queried using the Win32 API.
            var target = _hwndSource.CompositionTarget;
            if (target == null) {
                throw new NullReferenceException("target");
            }

            _matrix = target.TransformToDevice;

            SetExtendedToolWindowStyle();
        }

        private void SetExtendedToolWindowStyle() {
            var exStyle = (int)NativeMethods.GetWindowLong(_hwndSource.Handle, (int)GetWindowLongFields.GwlExstyle);
            exStyle |= (int)ExtendedWindowStyles.WsExToolwindow;
            SetWindowLong(_hwndSource.Handle, (int)GetWindowLongFields.GwlExstyle, (IntPtr)exStyle);
        }

        public Dock DockPosition {
            get { return (Dock)GetValue(DockPositionProperty); }
            set { SetValue(DockPositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DockPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DockPositionProperty =
            DependencyProperty.Register("DockPosition", typeof(Dock), typeof(ShadowCaster), new PropertyMetadata(Dock.Left));

        #region Native Window Support

        [Flags]
        public enum ExtendedWindowStyles {
            WsExToolwindow = 0x00000080,
        }

        public enum GetWindowLongFields {
            GwlExstyle = (-20),
        }

        private static int IntPtrToInt32(IntPtr intPtr) {
            return unchecked((int)intPtr.ToInt64());
        }

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong) {
            int error;
            IntPtr result;
            // Win32 SetWindowLong doesn't clear error on success
            NativeMethods.SetLastError(0);

            if (IntPtr.Size == 4) {
                // use SetWindowLong
                var tempResult = NativeMethods.IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            } else {
                // use SetWindowLongPtr
                result = NativeMethods.IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0)) {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }


        private static class NativeMethods {
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

            [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
            public static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

            [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
            public static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

            [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
            public static extern void SetLastError(int dwErrorCode);
        }

        #endregion
    }
}

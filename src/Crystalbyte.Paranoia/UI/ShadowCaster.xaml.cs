using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for ShadowCaster.xaml
    /// </summary>
    public partial class ShadowCaster {

        #region Private Fields

        private HwndSource _hwndSource;

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
                    DockLeft(window);
                    break;
                case Dock.Top:
                    DockLeft(window);
                    break;
                case Dock.Bottom:
                    DockLeft(window);
                    break;
            }
        }

        private void DockLeft(Window window) {
            Caster.Width = 1;
            Caster.HorizontalAlignment = HorizontalAlignment.Right;
            Caster.VerticalAlignment = VerticalAlignment.Stretch;
            Left = window.Left - 15;
            Top = window.Top;
            Width = 10;
            Height = window.Height;
            DropShadowEffect.Direction = 180;
        }

        private void OnSourceInitialized(object sender, EventArgs e) {
            var helper = new WindowInteropHelper(this);
            _hwndSource = HwndSource.FromHwnd(helper.Handle);

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
            var error = 0;
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

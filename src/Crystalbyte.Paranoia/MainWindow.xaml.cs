#region Using directives

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Crystalbyte.Paranoia.Contexts;
using Crystalbyte.Paranoia.Cryptography;
using System.ComponentModel;
using Crystalbyte.Paranoia.Models;

#endregion

namespace Crystalbyte.Paranoia {
    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        private HwndSource _source;

        public MainWindow() {
            if (DesignerProperties.GetIsInDesignMode(this)) {
                return;
            }

            DataContext = App.AppContext;
            InitializeComponent();
            Loaded += OnLoaded;
            // We need to set the height for the window to stay ontop the Taskbar
            MaxHeight = SystemParameters.WorkArea.Height
                        + SystemParameters.WindowResizeBorderThickness.Top
                        + SystemParameters.WindowResizeBorderThickness.Bottom;
        }

        public bool IsNormalState {
            get { return (bool)GetValue(IsNormalStateProperty); }
            set { SetValue(IsNormalStateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsNormalState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsNormalStateProperty =
            DependencyProperty.Register("IsNormalState", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));

        protected override void OnStateChanged(EventArgs e) {
            base.OnStateChanged(e);
            UpdateWindowPadding();
            IsNormalState = WindowState == WindowState.Normal;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            try {
                HookEntropyGenerator();
            } catch (Exception) {
                // TODO: We are probably offline, deal with it.
                throw;
            }
        }

        private void HookEntropyGenerator() {
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            if (_source == null) {
                throw new NullReferenceException("HwndSource must not be null.");
            }
            _source.AddHook(WndProc);
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            if (!OpenSslRandom.IsSeededSufficiently) {
                OpenSslRandom.AddEntropyFromEvents(msg, wParam, lParam);
            }
            return IntPtr.Zero;
        }

        private void OnMailSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var context = DataContext as AppContext;
            if (context == null) {
                return;
            }

            var list = sender as ListView;
            if (list == null) {
                return;
            }

            context.MailSelectionSource.Mails.Clear();
            context.MailSelectionSource.Mails.AddRange(list.SelectedItems.OfType<MailContext>());
        }

        private void OnCloseButtonClicked(object sender, RoutedEventArgs e) {
            Close();
        }

        private void UpdateWindowPadding() {
            if (WindowState == WindowState.Normal) {
                Padding = new Thickness(0);
            } else {
                Padding = new Thickness(
                    SystemParameters.WindowResizeBorderThickness.Left +
                    SystemParameters.WindowNonClientFrameThickness.Left,
                    SystemParameters.WindowResizeBorderThickness.Top +
                    SystemParameters.WindowNonClientFrameThickness.Top - SystemParameters.CaptionHeight,
                    SystemParameters.WindowResizeBorderThickness.Right +
                    SystemParameters.WindowNonClientFrameThickness.Right, 0);
            }
        }

        private void OnMaximizeButtonClicked(object sender, RoutedEventArgs e) {
            ToggleWindowState();
        }

        private void ToggleWindowState() {
            if (WindowState == WindowState.Normal) {
                MaximizeWindow();
            } else {
                NormalizeWindow();
            }
        }

        private void NormalizeWindow() {
            WindowState = WindowState.Normal;
        }

        private void OnMinimizeButtonClicked(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeWindow() {
            WindowState = WindowState.Maximized;
        }

        private void OnIdentitySelectionChanged(object sender, SelectionChangedEventArgs e) {
            var context = DataContext as AppContext;
            if (context == null) {
                return;
            }

            context.IdentitySelectionSource.Identity = e.AddedItems.OfType<IdentityContext>().FirstOrDefault();
        }

        private void OnContactSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var context = DataContext as AppContext;
            if (context == null) {
                return;
            }

            context.ContactSelectionSource.Contact = e.AddedItems.OfType<ContactContext>().FirstOrDefault();
        }

        private void OnMailboxSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var context = DataContext as AppContext;
            if (context == null) {
                return;
            }

            var mails = context.MailSelectionSource.Mails.ToArray();
            mails.ForEach(x => x.IsSelected = false);

            context.MailSelectionSource.Mails.Clear();
            context.MailboxSelectionSource.Mailbox = e.AddedItems.OfType<MailboxContext>().FirstOrDefault();
        }
    }
}
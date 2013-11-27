#region Using directives

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Crystalbyte.Paranoia.Contexts;
using Crystalbyte.Paranoia.Cryptography;
using Crystalbyte.Paranoia.Messaging.Mime;

#endregion

namespace Crystalbyte.Paranoia {
    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        private HwndSource _source;

        public MainWindow() {
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

        private async void OnLoaded(object sender, RoutedEventArgs e) {
            try {
                HookEntropyGenerator();
            }
            catch (Exception) {
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

        private async void OnMessagesSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var list = (ListView)sender;
            var messages = list.SelectedItems.OfType<ImapMessageContext>().ToList();
            if (messages.Count == 0) {
                return;
            }

            //ImapMessageSelectionSource.ChangeSelection(messages);
            var first = e.AddedItems.OfType<ImapMessageContext>().FirstOrDefault();
            if (first == null)
                return;

            first.ReadAsync();
            //var mime = await first.FetchContentAsync();
            //using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(mime))) {
            //    var message = Message.Load(stream);
            //    var html = message.FindFirstHtmlVersion();
            //    if (html != null) {
            //        App.AppContext.MessageBody = html.GetBodyAsText();
            //    } else {
            //        var plain = message.FindFirstPlainTextVersion();
            //        if (plain != null) {
            //            App.AppContext.MessageBody = plain.GetBodyAsText();
            //        }
            //    }
            //}
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
    }
}
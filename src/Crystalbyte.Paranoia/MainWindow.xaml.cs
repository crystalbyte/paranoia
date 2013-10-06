using System.IO;
using Crystalbyte.Paranoia.Contexts;
using Crystalbyte.Paranoia.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Crystalbyte.Paranoia.Messaging.Mime;

namespace Crystalbyte.Paranoia {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {

        public MainWindow() {
            InitializeComponent();

            Loaded += OnLoaded;
            DataContext = App.AppContext;

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

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e) {
            base.OnMouseDoubleClick(e);
            ToggleWindowState();
        }

        private static async void OnLoaded(object sender, RoutedEventArgs e) {
            await App.AppContext.SyncAsync();
        }

        private async void OnMessagesSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var context = e.AddedItems.OfType<MessageContext>().FirstOrDefault();
            if (context == null)
                return;

            var mime = await context.FetchMessageBodyAsync();
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(mime))) {
                var message = Message.Load(stream);
                App.AppContext.MessageBody = message.FindFirstHtmlVersion().GetBodyAsText();
            }
        }

        private void OnCloseButtonClicked(object sender, RoutedEventArgs e) {
            Close();
        }

        private void UpdateWindowPadding() {
            if (WindowState == WindowState.Normal) {
                Padding = new Thickness(0);
            } else {
                Padding = new Thickness(
                    SystemParameters.WindowResizeBorderThickness.Left + SystemParameters.WindowNonClientFrameThickness.Left,
                    SystemParameters.WindowResizeBorderThickness.Top + SystemParameters.WindowNonClientFrameThickness.Top - SystemParameters.CaptionHeight,
                    SystemParameters.WindowResizeBorderThickness.Right + SystemParameters.WindowNonClientFrameThickness.Right, 0);
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

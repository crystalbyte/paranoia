#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow {
        private readonly IEnumerable<MailMessageContext> _messages;
        private Storyboard _entryStoryboard;
        private Storyboard _exitStoryboard;

        public NotificationWindow(ICollection<MailMessageContext> messages) {
            _messages = messages;

            InitializeComponent();
            DataContext = new NotificationWindowContext(messages);
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            _entryStoryboard.Begin();
        }

        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);

            Left = SystemParameters.WorkArea.Width - Width - 20;
            Top = SystemParameters.WorkArea.Top + 20;

            _entryStoryboard = (Storyboard)Resources["EntryAnimation"];
            _entryStoryboard.Completed += OnEntryStoryboardCompleted;

            _exitStoryboard = (Storyboard)Resources["ExitAnimation"];
            _exitStoryboard.Completed += OnExitAnimationCompleted;
        }

        private void SlideOut() {
            _exitStoryboard.Begin();
        }

        private void OnEntryStoryboardCompleted(object sender, EventArgs e) {
            SlideOut();
        }

        private void OnCloseButtonClicked(object sender, RoutedEventArgs e) {
            SlideOut();
        }

        private void OnWindowMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (!_messages.Any()) {
                return;
            }

            App.Context.ShowMessage(_messages.First());
            SlideOut();
        }

        private void OnExitAnimationCompleted(object sender, EventArgs e) {
            Close();
        }
    }
}
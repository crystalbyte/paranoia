#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Crystalbyte.Paranoia.Data;

#endregion

namespace Crystalbyte.Paranoia {
    /// <summary>
    ///     Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow {
        private readonly IEnumerable<MailMessageContext> _messages;
        private Storyboard _storyboard;

        public NotificationWindow(ICollection<MailMessageContext> messages) {
            _messages = messages;

            InitializeComponent();
            DataContext = new NotificationWindowContext(messages);
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            _storyboard.Begin();
        }

        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);

            Left = SystemParameters.WorkArea.Width - Width;
            Top = SystemParameters.WorkArea.Top + 20;

            _storyboard = (Storyboard) Resources["EntryAnimation"];
            _storyboard.Completed += OnStoryboardCompleted;
        }

        private void OnStoryboardCompleted(object sender, EventArgs e) {
            Close();
        }

        private void OnCloseButtonClicked(object sender, RoutedEventArgs e) {
            Close();
        }

        private void OnWindowMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (!_messages.Any()) {
                return;
            }

            App.Context.ShowMessage(_messages.First());
            Close();
        }
    }
}
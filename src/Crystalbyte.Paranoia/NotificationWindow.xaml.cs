#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;
using Crystalbyte.Paranoia.Data;

#endregion

namespace Crystalbyte.Paranoia {
    /// <summary>
    ///     Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow {
        private Storyboard _storyboard;

        public NotificationWindow(IList<MailMessageModel> mails) {
            InitializeComponent();
            DataContext = new NotificationWindowContext(mails);
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
    }
}
#region Using directives

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Crystalbyte.Paranoia.Data;

#endregion

namespace Crystalbyte.Paranoia {
    /// <summary>
    ///     Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow {
        public NotificationWindow(List<MailMessageModel> mails) {
            DataContext = new NotificationWindowContext(mails);
            InitializeComponent();
            var sb = (Storyboard) Resources["NotificationStoryboard"];

            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => {
                                                                                      var workingArea =
                                                                                          SystemParameters.WorkArea;
                                                                                      Left = workingArea.Right -
                                                                                             ActualWidth;
                                                                                      Top = workingArea.Top;
                                                                                  }));
            sb.Completed += OnStoryboardCompleted;
        }

        private void OnStoryboardCompleted(object sender, EventArgs e) {
            Close();
        }
    }
}
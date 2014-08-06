using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Crystalbyte.Paranoia
{
    /// <summary>
    /// Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow
    {
        public NotificationWindow()
        {
            InitializeComponent();
            var sb = (Storyboard)Resources["NotificationStoryboard"];

            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                var workingArea = System.Windows.SystemParameters.WorkArea;
                this.Left = workingArea.Right - this.ActualWidth;
                this.Top = workingArea.Bottom - this.ActualHeight;
            }));
            sb.Completed += OnStoryboardCompleted;
        }

        private void OnStoryboardCompleted(object sender, EventArgs e)
        {
            Close();
        }
    }
}

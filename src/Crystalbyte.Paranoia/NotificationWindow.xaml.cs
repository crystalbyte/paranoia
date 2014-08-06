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

            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                var workingArea = System.Windows.SystemParameters.WorkArea;
                //var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
                //var corner = transform.Transform(new Point(workingArea.Right, workingArea.Bottom));

                //this.Left = corner.X - this.ActualWidth - 100;
                //this.Top = corner.Y - this.ActualHeight;
                this.Left = workingArea.Right - this.ActualWidth - 100;
                this.Top = workingArea.Bottom - this.ActualHeight;
            }));
        }
    }
}

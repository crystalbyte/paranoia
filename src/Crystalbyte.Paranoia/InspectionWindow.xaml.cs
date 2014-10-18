using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace Crystalbyte.Paranoia {
    /// <summary>
    /// Interaktionslogik für InspectMessageWindow.xaml
    /// </summary>
    public partial class InspectionWindow {
        public InspectionWindow() {
            InitializeComponent();
        }

        public InspectionWindow(FileSystemInfo file)
            : this() {
            blub.Source = string.Format("asset://paranoia/file?path={0}", Uri.EscapeDataString(file.FullName));
        }

        public InspectionWindow(MailMessageContext message)
            : this() {
            blub.Source = string.Format("asset://paranoia/message/{0}", message.Id);
        }
    }
}

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

namespace Crystalbyte.Paranoia.UI.Pages {
    /// <summary>
    /// Interaction logic for WriteMessagePage.xaml
    /// </summary>
    public partial class ComposeMessagePage : Page {

        public ComposeMessagePage() {
            DataContext = new MailCompositionContext();
            InitializeComponent();
        }

        private void OnAutoCompleteBoxOnItemsSourceRequested(object sender, EventArgs e) {
            
        }
    }
}

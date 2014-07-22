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
    public partial class ComposeMessagePage {

        public ComposeMessagePage() {
            DataContext = new MailCompositionContext();
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            SubjectTextBox.Focus();
        }

        private async void OnAutoCompleteBoxOnItemsSourceRequested(object sender, EventArgs e) {
            await ((MailCompositionContext) DataContext).QueryRecipientsAsync();
        }
    }
}

using System.ComponentModel;
using System.Windows;
using Crystalbyte.Paranoia.Contexts;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for ServerConfigPage.xaml
    /// </summary>
    public partial class ServerConfigTestingPage {
        public ServerConfigTestingPage() {
            InitializeComponent();
        }
        private void OnPortPreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e) {
            if (!char.IsDigit(e.Text, e.Text.Length - 1)) {
                e.Handled = true;
            }
        }
    }
}

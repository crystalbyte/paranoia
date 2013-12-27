using System.Security;
using Crystalbyte.Paranoia.Contexts;
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

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for CreateAccountStartPage.xaml
    /// </summary>
    public partial class CreateAccountStartPage {
        public CreateAccountStartPage() {
            InitializeComponent();
            ScreenContext = App.AppContext.AccountScreenContext;
            ScreenContext.Activated += OnActivated;
        }

        private void OnActivated(object sender, EventArgs e) {
            var identity = App.AppContext.Identities.FirstOrDefault(x => x.IsSelected);
            if (identity != null) {
                EmailAddressField.SelectedText = identity.EmailAddress;
            }
        }

        public AccountScreenContext ScreenContext {
            get { return DataContext as AccountScreenContext; }
            set { DataContext = value; }
        }

        private void OnPortPreviewTextInput(object sender, TextCompositionEventArgs e) {
            if (!char.IsDigit(e.Text, e.Text.Length - 1)) {
                e.Handled = true;
            }
        }
    }
}

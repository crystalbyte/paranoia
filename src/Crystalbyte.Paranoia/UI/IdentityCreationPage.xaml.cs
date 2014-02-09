#region Using directives

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Crystalbyte.Paranoia.Contexts;
using System.Windows.Data;
using System.Windows.Navigation;
using System.Windows.Documents;
using System.Diagnostics;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///   Interaction logic for CreateAccountStartPage.xaml
    /// </summary>
    public partial class IdentityCreationPage {
        public IdentityCreationPage() {
            InitializeComponent();
        }

        private void OnActivated(object sender, EventArgs e) {
            var identity = App.AppContext.Identities.FirstOrDefault(x => x.IsSelected);
            //if (identity != null && !string.IsNullOrWhiteSpace(ScreenContext.EmailAddress)) {
            //    EmailAddressField.SelectedText = identity.EmailAddress;
            //}
        }

        //public CreateAccountScreenContext ScreenContext {
        //    get { return DataContext as CreateAccountScreenContext; }
        //    set { DataContext = value; }
        //}

        private void OnImapPasswordChanged(object sender, RoutedEventArgs e) {
            var box = sender as PasswordBox;
            if (box == null) {
                return;
            }

            // The PasswordBox clears its content after being collected, however our password must persist to the next page.
            if (string.IsNullOrWhiteSpace(box.Password)) {
                return;
            }

            //ScreenContext.ImapPassword = box.Password;
        }

        private void OnGravatarLinkRequestNavigate(object sender, RequestNavigateEventArgs e) {
            var hyperlink = sender as Hyperlink;
            if (hyperlink == null) {
                return;
            }

            Process.Start(hyperlink.NavigateUri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
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
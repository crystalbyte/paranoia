#region Using directives

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Crystalbyte.Paranoia.Contexts;
using System.Windows.Data;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///   Interaction logic for CreateAccountStartPage.xaml
    /// </summary>
    public partial class CreateAccountStartPage {
        public CreateAccountStartPage() {
            ScreenContext = App.AppContext.CreateAccountScreenContext;
            ScreenContext.Activated += OnActivated;
            Loaded += OnPageLoaded;
            InitializeComponent();
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e) {
            
        }

        private void OnActivated(object sender, EventArgs e) {
            var identity = App.AppContext.Identities.FirstOrDefault(x => x.IsSelected);
            if (identity != null) {
                EmailAddressField.SelectedText = identity.EmailAddress;
            }
        }

        public CreateAccountScreenContext ScreenContext {
            get { return DataContext as CreateAccountScreenContext; }
            set { DataContext = value; }
        }

        private void OnImapPasswordChanged(object sender, RoutedEventArgs e) {
            var box = sender as PasswordBox;
            if (box == null) {
                return;
            }

            // The PasswordBox clears its content after being collected, however our password must persist to the next page.
            if (string.IsNullOrWhiteSpace(box.Password)) {
                return;
            }

            ScreenContext.ImapPassword = box.Password;
        }
    }
}
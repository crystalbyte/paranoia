#region Using directives

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

#endregion

namespace Crystalbyte.Paranoia {
    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {

        #region Construction

        public MainWindow() {
            DataContext = App.Context;
            InitializeComponent();
        }

        #endregion

        #region Class Overrides

        protected async override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);

            await App.Context.RunAsync();
            AccountsSource = App.Context.Accounts;
        }

        #endregion

        private void OnLoaded(object sender, RoutedEventArgs e) {
            try {

                HookEntropyGenerator();
            } catch (Exception) {
                // TODO: We are probably offline or hit the quota, deal with it.
                throw;
            }
        }

        private void HookEntropyGenerator() {
            var helper = new WindowInteropHelper(this);
        }

        private void OnContactsSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var source = App.Composition.GetExport<MailContactSelectionSource>();
            source.Selection = e.AddedItems.OfType<MailContactContext>();
        }

        private void OnMailboxSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var source = App.Composition.GetExport<MailboxSelectionSource>();
            source.Selection = e.AddedItems.OfType<MailboxContext>();
        }
    }
}
#region Using directives

using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

        private void OnAccountsSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var source = App.Composition.GetExport<MailAccountSelectionSource>();
            source.Selection = e.AddedItems.OfType<MailAccountContext>();
        }

        private void OnContactsSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selector = (ListView)sender;
            var source = App.Composition.GetExport<MailContactSelectionSource>();
            source.Selection = selector.SelectedItems.OfType<MailContactContext>().ToArray();
        }

        private void OnMailboxSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selector = (ListView)sender;
            var source = App.Composition.GetExport<MailboxSelectionSource>();
            source.Selection = selector.SelectedItems.OfType<MailboxContext>().ToArray();
        }
    }
}
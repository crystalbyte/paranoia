#region Using directives

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Interop;

#endregion

namespace Crystalbyte.Paranoia {
    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {

        #region Construction

        public MainWindow() {
            DataContext = App.Foundation;
        }

        #endregion

        #region Class Overrides

        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);
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
    }
}
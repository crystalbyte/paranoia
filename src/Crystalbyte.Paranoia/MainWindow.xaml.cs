#region Using directives

using System;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
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

        private void OnMessageSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var view = (ListView)sender;
            var app = App.Composition.GetExport<AppContext>();
            app.SelectedMessages = view.SelectedItems.OfType<MailMessageContext>();
        }
    }
}
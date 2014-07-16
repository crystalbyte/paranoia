#region Using directives

using System;
using System.Linq;
using System.Windows.Controls;

#endregion

namespace Crystalbyte.Paranoia {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        #region Construction

        public MainWindow() {
            DataContext = App.Context;
            InitializeComponent();
        }

        #endregion

        #region Class Overrides

        protected override async void OnInitialized(EventArgs e) {
            base.OnInitialized(e);
            App.Context.HookUpSearchBox(SearchBox);
            await App.Context.RunAsync();
        }

        #endregion

        private void OnMessageSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var view = (ListView) sender;
            var app = App.Composition.GetExport<AppContext>();
            app.SelectedMessages = view.SelectedItems.OfType<MailMessageContext>();

            var message = app.SelectedMessage;
            if (message == null) 
                return;

            var container = (Control) MessagesListView.ItemContainerGenerator.ContainerFromItem(message);
            if (container != null) {
                container.Focus();
            }
        }
    }
}
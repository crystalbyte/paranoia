using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NLog;

namespace Crystalbyte.Paranoia.UI.Pages {
    /// <summary>
    /// Interaction logic for MessagesPage.xaml
    /// </summary>
    public partial class MessagesPage {

        private readonly static Logger Logger = LogManager.GetCurrentClassLogger();

        public MessagesPage() {
            InitializeComponent();
            DataContext = App.Context;
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            try {
                DataContext = null;
                HtmlControl.Dispose();
            }
            catch (Exception ex) {
                Logger.Error(ex);        
            }
        }

        private async void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {

            if (!IsLoaded) {
                return;
            }

            var tree = (TreeView)sender;
            var value = tree.SelectedValue;

            var mailbox = value as MailboxContext;
            if (mailbox != null) {
                App.Context.SelectedMailbox = mailbox;
            }

            var account = value as MailAccountContext;
            if (account == null)
                return;

            if (!account.IsOnline) {
                await account.TakeOnlineAsync();
            }
        }

        private void OnMessageSelectionChanged(object sender, SelectionChangedEventArgs e) {

            if (!IsLoaded) {
                return;
            }

            var app = App.Context;
            app.OnMessageSelectionChanged();

            var message = app.SelectedMessage;
            if (message == null)
                return;

            var container = (Control)MessagesListView.ItemContainerGenerator.ContainerFromItem(message);
            if (container != null) {
                container.Focus();
            }
        }

        private void OnAttachmentMouseDoubleClicked(object sender, MouseButtonEventArgs e) {
            if (!IsLoaded) {
                return;
            }
            var view = (ListView)sender;
            var attachment = (AttachmentContext)view.SelectedValue;
            if (attachment == null) {
                return;
            }
            attachment.Open();
        }
    }
}

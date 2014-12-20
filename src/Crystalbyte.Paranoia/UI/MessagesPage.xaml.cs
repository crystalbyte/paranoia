using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using NLog;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for MessagesPage.xaml
    /// </summary>
    public partial class MessagesPage {

        private readonly static Logger Logger = LogManager.GetCurrentClassLogger();

        public MessagesPage() {
            InitializeComponent();
            DataContext = App.Context;
            Unloaded += OnUnloaded;

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Print, OnPrint, OnCanPrint));
            CommandBindings.Add(new CommandBinding(MessageCommands.Compose, OnCompose));
            CommandBindings.Add(new CommandBinding(MessageCommands.Reply, OnReply));
            CommandBindings.Add(new CommandBinding(MessageCommands.ReplyAll, OnReplyAll));
            CommandBindings.Add(new CommandBinding(MessageCommands.Forward, OnForward));
            CommandBindings.Add(new CommandBinding(MessageCommands.Inspect, OnInspect, OnCanInspect));
            CommandBindings.Add(new CommandBinding(OutboxCommands.Delete, OnDeleteSmtpRequest, OnCanDeleteSmtpRequest));
            App.Context.SortOrderChanged += OnSortOrderChanged;
        }

        private static void OnCanInspect(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = App.Context.SelectedMessage != null;
        }

        private void OnCanDeleteSmtpRequest(object sender, CanExecuteRoutedEventArgs e) {

        }

        private void OnDeleteSmtpRequest(object sender, ExecutedRoutedEventArgs e) {

        }

        private static void OnForward(object sender, ExecutedRoutedEventArgs e) {
            try {
                App.Context.Forward();
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private static void OnReply(object sender, ExecutedRoutedEventArgs e) {
            try {
                App.Context.Reply();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private static void OnReplyAll(object sender, ExecutedRoutedEventArgs e) {
            try {
                App.Context.ReplyToAll();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private static void OnCompose(object sender, ExecutedRoutedEventArgs e) {
            try {
                App.Context.Compose();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnPrint(object sender, ExecutedRoutedEventArgs e) {
            var html = MessageViewer.GetDocument();
            App.Context.Print(html);
        }

        private void OnCanPrint(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = MessagesListView.SelectedValue != null;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            try {
                App.Context.SortOrderChanged -= OnSortOrderChanged;
                DataContext = null;
                MessageViewer.Dispose();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnSortOrderChanged(object sender, EventArgs e) {
            var source = Resources["MessagesSource"] as CollectionViewSource;
            if (source == null)
                return;

            source.SortDescriptions.Clear();
            source.SortDescriptions.Add(new SortDescription("EntryDate", App.Context.IsSortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending));
        }

        private async void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            if (!IsLoaded) {
                return;
            }

            var tree = (TreeView)sender;
            var value = tree.SelectedValue;

            App.Context.SelectedOutbox = value as OutboxContext;
            App.Context.SelectedMailbox = value as MailboxContext;

            var account = value as MailAccountContext;
            if (account == null)
                return;

            if (account.TakeOnlineHint) {
                await account.TakeOnlineAsync();
            }
        }

        private void OnSmtpRequestSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!IsLoaded) {
                return;
            }

            var outbox = App.Context.SelectedOutbox;
            if (outbox == null) {
                return;
            }

            outbox.OnSmtpRequestSelectionChanged();

            CommandManager.InvalidateRequerySuggested();
            var request = outbox.SelectedSmtpRequest;
            if (request == null) {
                return;
            }

            var container = (Control)SmtpRequestsListView.ItemContainerGenerator.ContainerFromItem(request);
            if (container != null) {
                container.Focus();
            }
        }

        private void OnMessageSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!IsLoaded) {
                return;
            }

            foreach (var item in e.AddedItems.OfType<SelectionObject>()) {
                item.IsSelected = true;
            }

            foreach (var item in e.RemovedItems.OfType<SelectionObject>()) {
                item.IsSelected = false;
            }

            var app = App.Context;
            app.OnMessageSelectionChanged();

            CommandManager.InvalidateRequerySuggested();

            var message = app.SelectedMessage;
            if (message == null)
                return;

            var container = (Control)MessagesListView
                .ItemContainerGenerator.ContainerFromItem(message);
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

        private static async void OnInspect(object sender, ExecutedRoutedEventArgs e) {
            await App.Context.InspectMessageAsync(App.Context.SelectedMessage);
        }

        private async void OnMessageMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            await App.Context.InspectMessageAsync(App.Context.SelectedMessage);
        }

        private void OnTreeViewGotFocus(object sender, RoutedEventArgs e) {
            App.Context.Accounts.ForEach(x => x.IsKeyboardFocused = true);
        }

        private void OnTreeViewLostFocus(object sender, RoutedEventArgs e) {
            App.Context.Accounts.ForEach(x => x.IsKeyboardFocused = false);
        }
    }
}

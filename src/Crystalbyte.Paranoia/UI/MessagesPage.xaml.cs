using System;
using System.ComponentModel;
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
            CommandBindings.Add(new CommandBinding(MessagingCommands.Compose, OnCompose));
            CommandBindings.Add(new CommandBinding(MessagingCommands.Reply, OnReply));
            CommandBindings.Add(new CommandBinding(MessagingCommands.ReplyAll, OnReplyAll));
            CommandBindings.Add(new CommandBinding(MessagingCommands.Forward, OnForward));
            CommandBindings.Add(new CommandBinding(MessagingCommands.Inspect, OnInspect, OnCanInspect));
            //CommandBindings.Add(new CommandBinding(OutboxCommands.Delete, OnDeleteSmtpRequest, OnCanDeleteSmtpRequest));
            App.Context.SortOrderChanged += OnSortOrderChanged;
        }

        public SortProperty SortProperty {
            get { return (SortProperty)GetValue(SortPropertyProperty); }
            set { SetValue(SortPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SortProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SortPropertyProperty =
            DependencyProperty.Register("SortProperty", typeof(SortProperty), typeof(MessagesPage), new PropertyMetadata(SortProperty.Date));

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
            } catch (Exception ex) {
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
            var app = App.Context;
            app.IsSortAscending = !app.IsSortAscending;
            var direction = app.IsSortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending;
            Sort(SortProperty, direction);
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

        private void OnSortPropertyButtonClicked(object sender, RoutedEventArgs e) {
            SortPropertyMenu.IsOpen = true;
        }

        private void OnSortPropertyMenuItemClicked(object sender, RoutedEventArgs e) {
            var item = (MenuItem)sender;
            var app = (AppContext)DataContext;
            var direction = app.IsSortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending;
            Sort((SortProperty)item.DataContext, direction);
        }

        private void Sort(SortProperty property, ListSortDirection direction) {

            SortProperty = property;
            var source = (CollectionViewSource)Resources["MessagesSource"];
            source.SortDescriptions.Clear();

            string name;
            switch (property) {
                case SortProperty.Size:
                    name = PropertySupport.ExtractPropertyName((MailMessageContext m) => m.Size);
                    break;
                case SortProperty.Attachments:
                    name = PropertySupport.ExtractPropertyName((MailMessageContext m) => m.HasAttachments);
                    break;
                case SortProperty.Subject:
                    name = PropertySupport.ExtractPropertyName((MailMessageContext m) => m.Subject);
                    break;
                case SortProperty.Date:
                    name = PropertySupport.ExtractPropertyName((MailMessageContext m) => m.EntryDate);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("property");

            }
            source.SortDescriptions.Add(new SortDescription(name, direction));
        }
    }
}

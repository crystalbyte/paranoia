using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Print, OnPrint, OnCanPrint));
            CommandBindings.Add(new CommandBinding(MessageCommands.Compose, OnCompose));
            CommandBindings.Add(new CommandBinding(MessageCommands.Reply, OnReply));
            CommandBindings.Add(new CommandBinding(MessageCommands.ReplyAll, OnReplyAll));
            CommandBindings.Add(new CommandBinding(MessageCommands.Forward, OnForward));
            CommandBindings.Add(new CommandBinding(MessageCommands.Resume, OnResume));
            CommandBindings.Add(new CommandBinding(MessageCommands.Inspect, OnInspect));
            CommandBindings.Add(new CommandBinding(OutboxCommands.Delete, OnDeleteSmtpRequest, OnCanDeleteSmtpRequest));
            App.Context.SortOrderChanged += OnSortOrderChanged;
        }

        private void OnCanDeleteSmtpRequest(object sender, CanExecuteRoutedEventArgs e) {

        }

        private void OnDeleteSmtpRequest(object sender, ExecutedRoutedEventArgs e) {

        }

        private static void OnResume(object sender, ExecutedRoutedEventArgs e) {
            var message = App.Context.SelectedMessage;
            if (message == null) {
                throw new InvalidOperationException();
            }

            var owner = Application.Current.MainWindow;
            var window = CreateComposeChildWindow(owner);

            var uri = string.Format("?action=resume&id={0}", message.Id);
            window.Source = typeof(ComposeMessagePage).ToPageUri(uri);

            if (owner.WindowState == WindowState.Maximized) {
                window.WindowState = WindowState.Maximized;
            }

            window.Show();
        }

        private static void OnForward(object sender, ExecutedRoutedEventArgs e) {
            var message = App.Context.SelectedMessage;
            if (message == null) {
                throw new InvalidOperationException();
            }

            var owner = Application.Current.MainWindow;
            var window = CreateComposeChildWindow(owner);

            var uri = string.Format("?action=forward&id={0}", message.Id);
            window.Source = typeof(ComposeMessagePage).ToPageUri(uri);

            if (owner.WindowState == WindowState.Maximized) {
                window.WindowState = WindowState.Maximized;
            }

            window.Show();
        }

        private static void OnReply(object sender, ExecutedRoutedEventArgs e) {
            var message = App.Context.SelectedMessage;
            if (message == null) {
                throw new InvalidOperationException();
            }

            var owner = Application.Current.MainWindow;
            var window = CreateComposeChildWindow(owner);

            var uri = string.Format("?action=reply&id={0}", message.Id);
            window.Source = typeof(ComposeMessagePage).ToPageUri(uri);

            if (owner.WindowState == WindowState.Maximized) {
                window.WindowState = WindowState.Maximized;
            }

            window.Show();
        }

        private static void OnReplyAll(object sender, ExecutedRoutedEventArgs e) {
            var message = App.Context.SelectedMessage;
            if (message == null) {
                throw new InvalidOperationException();
            }

            var owner = Application.Current.MainWindow;
            var window = CreateComposeChildWindow(owner);

            var uri = string.Format("?action=reply-all&id={0}", message.Id);
            window.Source = typeof(ComposeMessagePage).ToPageUri(uri);

            if (owner.WindowState == WindowState.Maximized) {
                window.WindowState = WindowState.Maximized;
            }

            window.Show();
        }

        private static void OnCompose(object sender, ExecutedRoutedEventArgs e) {
            var owner = Application.Current.MainWindow;
            var window = CreateComposeChildWindow(owner);
            window.Source = typeof(ComposeMessagePage).ToPageUri("?action=new");

            if (owner.WindowState == WindowState.Maximized) {
                window.WindowState = WindowState.Maximized;
            }

            window.Show();
        }

        private static CompositionWindow CreateComposeChildWindow(Window owner) {
            var window = new CompositionWindow {
                Height = owner.Height > 500 ? owner.Height * 0.9 : 500,
                Width = owner.Width > 800 ? owner.Width * 0.9 : 800,
            };

            var ownerPoint = owner.PointToScreen(new Point(0, 0));

            var left = ownerPoint.X + (owner.Width / 2) - (window.Width / 2);
            var top = ownerPoint.Y + (owner.Height / 2) - (window.Height / 2);
            window.Left = left < ownerPoint.X ? ownerPoint.X : left;
            window.Top = top < ownerPoint.Y ? ownerPoint.Y : top;
            owner.Closed += (sender, e) => window.Close();

            return window;
        }

        private void OnPrint(object sender, ExecutedRoutedEventArgs e) {
            var html = MessageViewer.GetDocument();

            var browser = new WebBrowser();
            browser.Navigated += (x, y) => {
                try {
                    dynamic document = browser.Document;
                    document.execCommand("print", true, null);
                } catch (Exception ex) {
                    Logger.Error(ex);
                } finally {
                    browser.Dispose();
                }
            };
            browser.NavigateToString(html);
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

            var app = App.Context;
            app.OnMessageSelectionChanged();

            CommandManager.InvalidateRequerySuggested();

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

        private void OnInspect(object sender, ExecutedRoutedEventArgs e) {
            var owner = Application.Current.MainWindow;
            var inspector = CreateInspectorChildWindow(owner);
            inspector.Show();
        }

        private static InspectMessageWindow CreateInspectorChildWindow(Window owner) {
            var message = App.Context.SelectedMessage;

            var window = new InspectMessageWindow(message) {
                Height = owner.Height > 500 ? owner.Height * 0.9 : 500,
                Width = owner.Width > 800 ? owner.Width * 0.9 : 800,
            };

            var ownerPoint = owner.PointToScreen(new Point(0, 0));

            var left = ownerPoint.X + (owner.Width / 2) - (window.Width / 2);
            var top = ownerPoint.Y + (owner.Height / 2) - (window.Height / 2);
            window.Left = left < ownerPoint.X ? ownerPoint.X : left;
            window.Top = top < ownerPoint.Y ? ownerPoint.Y : top;
            owner.Closed += (sender, e) => window.Close();

            return window;
        }
    }
}

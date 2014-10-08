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
            App.Context.SortOrderChanged += OnSortOrderChanged;
        }

        private static void OnForward(object sender, ExecutedRoutedEventArgs e) {
            var message = App.Context.SelectedMessage;
            if (message == null) {
                throw new InvalidOperationException();
            }

            var owner = Application.Current.MainWindow;
            var window = CreateChildWindow(owner);

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
            var window = CreateChildWindow(owner);

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
            var window = CreateChildWindow(owner);

            var uri = string.Format("?action=reply-all&id={0}", message.Id);
            window.Source = typeof(ComposeMessagePage).ToPageUri(uri);

            if (owner.WindowState == WindowState.Maximized) {
                window.WindowState = WindowState.Maximized;
            }

            window.Show();
        }

        private static void OnCompose(object sender, ExecutedRoutedEventArgs e) {
            var owner = Application.Current.MainWindow;
            var window = CreateChildWindow(owner);
            window.Source = typeof(ComposeMessagePage).ToPageUri("?action=new");

            if (owner.WindowState == WindowState.Maximized) {
                window.WindowState = WindowState.Maximized;
            }

            window.Show();
        }

        private static CompositionWindow CreateChildWindow(Window owner) {
            var window = new CompositionWindow {
                Height = owner.Height > 500 ? owner.Height * 0.9 : 500,
                Width = owner.Width > 800 ? owner.Width * 0.9 : 800,
            };

            var ownerPoint = owner.PointToScreen(new Point(0, 0));
            var left = ownerPoint.X + (owner.Width / 2) - (window.Width / 2);
            var top = ownerPoint.Y + (owner.Height / 2) - (window.Height / 2);
            window.Left = left < 0 ? 0 : left;
            window.Top = top < 0 ? 0 : top;
            owner.Closed += (sender, e) => window.Close();

            return window;
        }

        private void OnPrint(object sender, ExecutedRoutedEventArgs e) {
            var html = HtmlControl.GetDocument();

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
                HtmlControl.Dispose();
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
    }
}

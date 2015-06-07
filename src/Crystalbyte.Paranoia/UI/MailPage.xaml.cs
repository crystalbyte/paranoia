#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia
// 
// Crystalbyte.Paranoia is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License.
// 
// Foobar is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Using Directives

using System;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Crystalbyte.Paranoia.Data;
using NLog;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for MailPage.xaml
    /// </summary>
    public partial class MailPage {

        #region Private Fields

        private readonly CollectionViewSource _messageViewSource;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        public MailPage() {
            InitializeComponent();

            var context = App.Context;
            context.SortOrderChanged += OnSortOrderChanged;
            context.ItemSelectionRequested += OnItemSelectionRequested;
            DataContext = context;

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Print, OnPrint, OnCanPrint));
            CommandBindings.Add(new CommandBinding(MessageCommands.Compose, OnCompose));
            CommandBindings.Add(new CommandBinding(MessageCommands.Reply, OnReply));
            CommandBindings.Add(new CommandBinding(MessageCommands.ReplyAll, OnReplyAll));
            CommandBindings.Add(new CommandBinding(MessageCommands.Forward, OnForward));
            CommandBindings.Add(new CommandBinding(MessageCommands.Inspect, OnInspect, OnCanInspect));
            CommandBindings.Add(new CommandBinding(MessageCommands.QuickSearch, OnQuickSearch));
            CommandBindings.Add(new CommandBinding(MessageCommands.CancelSearch, OnCancelSearch));
            CommandBindings.Add(new CommandBinding(MailboxCommands.Create, OnCreateMailbox, OnCanCreateMailbox));
            CommandBindings.Add(new CommandBinding(MailboxCommands.Delete, OnDeleteMailbox, OnCanDeleteMailbox));
            CommandBindings.Add(new CommandBinding(MailboxCommands.Sync, OnSyncMailbox, OnCanSyncMailbox));

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            _messageViewSource = (CollectionViewSource)Resources["MessagesSource"];
        }

        private void OnCancelSearch(object sender, ExecutedRoutedEventArgs e) {
            try {
                var textBox = (WatermarkTextBox)e.OriginalSource;
                textBox.Text = string.Empty;

                var request = new TraversalRequest(FocusNavigationDirection.Previous);
                MoveFocus(request);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            try {
                AccountsTreeView.Focus();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnItemSelectionRequested(object sender, ItemSelectionRequestedEventArgs e) {
            if (_messageViewSource.View == null) {
                return;
            }

            try {
                var source = _messageViewSource.View.Cast<object>().ToList();
                if (e.Position == SelectionPosition.First) {
                    MessagesListView.SelectedIndex = 0;
                } else {
                    var index = e.PivotElements.GroupBy(source.IndexOf).Max(x => x.Key) + 1;
                    var item = MessagesListView.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem;
                    if (item != null) {
                        item.IsSelected = true;
                    }
                }
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private static void OnCanSyncMailbox(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = NetworkInterface.GetIsNetworkAvailable();
        }

        private static async void OnSyncMailbox(object sender, ExecutedRoutedEventArgs e) {
            if (e.Parameter == null) {
                return;
            }

            var mailbox = (MailboxContext)e.Parameter;
            if (!mailbox.IsSyncingMessages) {
                await mailbox.SyncMessagesAsync();
            }

            if (!mailbox.IsSyncingMailboxes) {
                await mailbox.SyncMailboxesAsync();
            }
        }

        private void OnQuickSearch(object sender, ExecutedRoutedEventArgs e) {
            QuickSearchBox.Focus();
        }

        private static void OnCanDeleteMailbox(object sender, CanExecuteRoutedEventArgs e) {
            var mailbox = (MailboxContext)e.Parameter;
            e.CanExecute = mailbox != null && mailbox.IsSelectable;
        }

        private static async void OnDeleteMailbox(object sender, ExecutedRoutedEventArgs e) {
            var mailbox = (MailboxContext)e.Parameter;
            await mailbox.DeleteAsync();
        }

        private static void OnCreateMailbox(object sender, ExecutedRoutedEventArgs e) {
            var parent = (IMailboxCreator)e.Parameter;
            App.Context.CreateMailbox(parent);
        }

        private static void OnCanCreateMailbox(object sender, CanExecuteRoutedEventArgs e) {
            var parent = (IMailboxCreator)e.Parameter;
            e.CanExecute = parent.CanHaveChildren;
        }

        public SortProperty SortProperty {
            get { return (SortProperty)GetValue(SortPropertyProperty); }
            set { SetValue(SortPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SortProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SortPropertyProperty =
            DependencyProperty.Register("SortProperty", typeof(SortProperty), typeof(MailPage),
                new PropertyMetadata(SortProperty.Date));

        private static void OnCanInspect(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = App.Context.SelectedMessage != null;
        }

        private static void OnForward(object sender, ExecutedRoutedEventArgs e) {
            App.Context.ForwardAsync();
        }

        private static void OnReply(object sender, ExecutedRoutedEventArgs e) {
            App.Context.ReplyAsync();
        }

        private static void OnReplyAll(object sender, ExecutedRoutedEventArgs e) {
            App.Context.ReplyToAllAsync();
        }

        private static void OnCompose(object sender, ExecutedRoutedEventArgs e) {
            App.Context.Compose();
        }

        private async void OnPrint(object sender, ExecutedRoutedEventArgs e) {
            await MessageViewer.PrintAsync();
        }

        private void OnCanPrint(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = MessagesListView.SelectedValue != null;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            App.Context.SortOrderChanged -= OnSortOrderChanged;
            DataContext = null;
        }

        private void OnSortOrderChanged(object sender, EventArgs e) {
            var app = App.Context;
            var direction = app.IsSortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending;
            Sort(SortProperty, direction);
        }

        private async void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            if (!IsLoaded) {
                return;
            }

            var tree = (TreeView)sender;
            var value = tree.SelectedValue;

            App.Context.SelectedMailbox = value as MailboxContext;

            RequeryRoutedCommands();

            var account = value as MailAccountContext;
            if (account == null)
                return;

            if (NetworkInterface.GetIsNetworkAvailable()) {
                await account.TakeOnlineAsync();
            }
        }

        private static void RequeryRoutedCommands() {
            CommandManager.InvalidateRequerySuggested();
        }

        private async void OnMessageSelectionChanged(object sender, SelectionChangedEventArgs e) {
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

            Task flagMessages = null;
            if (e.AddedItems.Count == 1) {
                var last = e.RemovedItems.OfType<MailMessageContext>().FirstOrDefault();
                if (last != null) {
                    flagMessages = app.MarkMessagesAsSeenAsync(new[] { last });
                }
            }

            var message = app.SelectedMessage;
            if (message == null)
                return;

            var container = (Control)MessagesListView
                .ItemContainerGenerator.ContainerFromItem(message);
            if (container != null) {
                container.Focus();
            }

            if (flagMessages != null) {
                await flagMessages;
            }
        }

        private void OnAttachmentMouseDoubleClicked(object sender, MouseButtonEventArgs e) {
            if (!IsLoaded) {
                return;
            }
            var view = (ListView)sender;
            var attachment = (MailAttachmentContext)view.SelectedValue;
            if (attachment == null) {
                return;
            }
            attachment.Open();
        }

        private static void OnInspect(object sender, ExecutedRoutedEventArgs e) {
            var message = e.Parameter as MailMessageContext;
            if (message == null) {
                return;
            }

            App.Context.InspectMessage(message);
        }

        private void OnMessageMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var item = sender as ListViewItem;
            if (item == null) {
                return;
            }

            var message = item.DataContext as MailMessageContext;
            if (message == null) {
                return;
            }

            // Need to invoke to let the event handler finish before opening a window.
            // http://stackoverflow.com/questions/14055794/wpf-treeview-restores-its-focus-after-double-click
            Dispatcher.InvokeAsync(() => App.Context.InspectMessage(message));
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
                    name = PropertySupport.ExtractPropertyName((MailMessageContext m) => m.Date);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("property");
            }
            source.SortDescriptions.Add(new SortDescription(name, direction));
        }

        #region Drag & Drop Support

        private Point _mousePosition;
        private DependencyObject _activeDragSource;

        private void OnPreviewMouseLeftButtonDownMessagesListView(object sender, MouseButtonEventArgs e) {
            var list = (ListView)sender;

            _mousePosition = e.GetPosition(list);
            VisualTreeHelper.HitTest(list, OnHitTestFilter, OnHitTestResult,
                new PointHitTestParameters(_mousePosition));
        }

        private static HitTestResultBehavior OnHitTestResult(HitTestResult result) {
            return HitTestResultBehavior.Stop;
        }

        private HitTestFilterBehavior OnHitTestFilter(DependencyObject target) {
            if (target is ListViewItem) {
                _activeDragSource = target;
                return HitTestFilterBehavior.Stop;
            }

            _activeDragSource = null;
            return HitTestFilterBehavior.Continue;
        }

        private void OnMouseLeaveMessagesListView(object sender, MouseEventArgs e) {
            _activeDragSource = null;
        }

        private void OnMouseMoveMessagesListView(object sender, MouseEventArgs e) {
            var list = (ListView)sender;
            if (_activeDragSource == null)
                return;

            var position = e.GetPosition(list);
            var diff = _mousePosition - position;

            if (e.LeftButton != MouseButtonState.Pressed ||
                !(Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                !(Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                return;

            if (MessagesListView.SelectedItems.Count == 0)
                return;

            var virtualFileObject = new VirtualFileDataObject();

            virtualFileObject.SetData((from MailMessageContext item in MessagesListView.SelectedItems
                                       select new VirtualFileDataObject.FileDescriptor {
                                           Name = GetValidFileName(item.Subject) + ".eml",
                                           Length = item.Size,
                                           ChangeTimeUtc = DateTime.UtcNow,
                                           StreamContents = async stream => {
                                               var bytes = await LoadMessageBytesAsync(item.Id);
                                               stream.Write(bytes, 0, bytes.Length);
                                           }
                                       }).ToArray());
            VirtualFileDataObject.DoDragDrop(sender as DependencyObject, virtualFileObject, DragDropEffects.Copy);
        }

        private static string GetValidFileName(string name) {
            return Path.GetInvalidFileNameChars().Aggregate(name, (current, c) => current.Replace(c, '_'));
        }

        private static async Task<byte[]> LoadMessageBytesAsync(Int64 id) {
            Logger.Enter();

            Application.Current.AssertBackgroundThread();

            try {
                using (var database = new DatabaseContext()) {
                    var mime = await database.MailMessages
                        .Where(x => x.Id == id)
                        .Select(x => x.Mime)
                        .FirstOrDefaultAsync();

                    if (mime != null)
                        return mime;

                    var text = string.Format(Paranoia.Properties.Resources.MissingMimeTemplate, id);
                    throw new InvalidOperationException(text);
                }

            } finally {
                Logger.Exit();
            }
        }

        #endregion
    }
}
﻿#region Copyright Notice & Copying Permission

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
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Crystalbyte.Paranoia.Data;
using NLog;
using System.Text;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for MailPage.xaml
    /// </summary>
    public partial class MailPage {

        #region Private Fields

        private MailMessageContext _toBeMarkedAsSeen;
        private readonly CollectionViewSource _messageViewSource;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public MailPage() {
            InitializeComponent();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            DataContext = App.Context.GetModule<MailModule>();

            _messageViewSource = (CollectionViewSource)Resources["MessagesSource"];
        }

        #endregion

        #region Class Override

        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);

            try {
                MessagesListView.SelectionChanged += OnMessageSelectionChanged;
                Observable.FromEventPattern<SelectionChangedEventHandler, SelectionChangedEventArgs>(
                    action => MessagesListView.SelectionChanged += action,
                    action => MessagesListView.SelectionChanged -= action)
                        .Throttle(TimeSpan.FromMilliseconds(200))
                        .Select(x => x.EventArgs)
                        .Subscribe(OnMessageSelectionFinalized);

                Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(
                    action => QuickSearchBox.TextChanged += action,
                    action => QuickSearchBox.TextChanged -= action)
                        .Select(x => ((WatermarkTextBox)x.Sender).Text)
                        .Where(x => x.Length > 2 || string.IsNullOrEmpty(x))
                        .Throttle(TimeSpan.FromMilliseconds(200))
                        .Subscribe(OnQueryTextChangeObserved);

            } catch (Exception ex) {
                Logger.Error(ex.Message, ex);
            }
        }

        private static void OnMessageSelectionChanged(object sender, SelectionChangedEventArgs e) {
            try {
                foreach (var item in e.RemovedItems.OfType<SelectionObject>()) {
                    item.IsSelected = false;
                }

                foreach (var item in e.AddedItems.OfType<SelectionObject>()) {
                    item.IsSelected = true;
                }

                var module = App.Context.GetModule<MailModule>();
                module.OnMessageSelectionChanged();

            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private static async void OnQueryTextChangeObserved(string query) {
            try {
                await Application.Current.Dispatcher.InvokeAsync(() => {
                    var module = App.Context.GetModule<MailModule>();
                    module.QueryMessages(query);
                });
            } catch (Exception ex) {
                Logger.Error(ex.Message, ex);
            }
        }

        private async void OnMessageSelectionFinalized(SelectionChangedEventArgs e) {
            try {
                await Application.Current.Dispatcher.InvokeAsync(async () => {
                    if (!IsLoaded) {
                        return;
                    }

                    var module = App.Context.GetModule<MailModule>();

                    Task flagMessages = null;
                    if (_toBeMarkedAsSeen != null) {
                        flagMessages = module.MarkMessagesAsSeenAsync(new[] { _toBeMarkedAsSeen });
                        _toBeMarkedAsSeen = null;
                    }

                    var message = module.SelectedMessage;
                    if (message == null)
                        return;

                    _toBeMarkedAsSeen = message;
                    await module.ViewMessageAsync(message);

                    //var container = (Control)MessagesListView
                    //    .ItemContainerGenerator.ContainerFromItem(message);
                    //if (container != null) {
                    //    container.Focus();
                    //}

                    if (flagMessages != null) {
                        await flagMessages;
                    }
                });
            } catch (Exception ex) {
                Logger.Error(ex.Message, ex);
            }
        }

        #endregion

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
                var module = App.Context.GetModule<MailModule>();
                module.SortOrderChanged += OnSortOrderChanged;
                module.ItemSelectionRequested += OnItemSelectionRequested;

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

        private void OnCanSyncMailbox(object sender, CanExecuteRoutedEventArgs e) {
            try {
                e.CanExecute = NetworkInterface.GetIsNetworkAvailable();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private async void OnSyncMailbox(object sender, ExecutedRoutedEventArgs e) {
            try {
                if (!(e.Parameter is MailboxContext)) {
                    return;
                }

                var mailbox = (MailboxContext)e.Parameter;
                if (!mailbox.IsSyncingMessages) {
                    await mailbox.SyncMessagesAsync();
                }

                if (!mailbox.IsSyncingMailboxes) {
                    await mailbox.SyncMailboxesAsync();
                }
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnQuickSearch(object sender, ExecutedRoutedEventArgs e) {
            try {
                QuickSearchBox.Focus();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnCanDeleteMailbox(object sender, CanExecuteRoutedEventArgs e) {
            try {
                var mailbox = (MailboxContext)e.Parameter;
                e.CanExecute = mailbox != null && mailbox.IsSelectable;
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private async void OnDeleteMailbox(object sender, ExecutedRoutedEventArgs e) {
            try {
                var mailbox = (MailboxContext)e.Parameter;
                await mailbox.DeleteAsync();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnCreateMailbox(object sender, ExecutedRoutedEventArgs e) {
            try {
                var parent = (IMailboxCreator)e.Parameter;
                var module = App.Context.GetModule<MailModule>();
                module.CreateMailbox(parent);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnCanCreateMailbox(object sender, CanExecuteRoutedEventArgs e) {
            try {
                var parent = (IMailboxCreator)e.Parameter;
                e.CanExecute = parent.CanHaveChildren;
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private async void OnShowSource(object sender, ExecutedRoutedEventArgs e) {
            try {
                var module = App.Context.GetModule<MailModule>();
                var message = module.SelectedMessage;
                if (message == null) {
                    return;
                }

                using (var context = new DatabaseContext()) {
                    var mime = await context.MailMessages
                        .Where(x => x.Id == message.Id)
                        .Select(x => x.Mime)
                        .FirstOrDefaultAsync();

                    var text = Encoding.UTF8.GetString(mime);

                    if (!string.IsNullOrEmpty(text)) {
                        Clipboard.SetText(text);
                    }

                }
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        public SortProperty SortProperty {
            get { return (SortProperty)GetValue(SortPropertyProperty); }
            set { SetValue(SortPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SortProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SortPropertyProperty =
            DependencyProperty.Register("SortProperty", typeof(SortProperty), typeof(MailPage),
                new PropertyMetadata(SortProperty.Date));

        private void OnCanInspect(object sender, CanExecuteRoutedEventArgs e) {
            try {
                var module = App.Context.GetModule<MailModule>();
                e.CanExecute = module.SelectedMessage != null;
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnForward(object sender, ExecutedRoutedEventArgs e) {
            try {
                var module = App.Context.GetModule<MailModule>();
                module.ForwardAsync();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnReply(object sender, ExecutedRoutedEventArgs e) {
            try {
                var module = App.Context.GetModule<MailModule>();
                module.ReplyAsync();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnReplyAll(object sender, ExecutedRoutedEventArgs e) {
            try {
                var module = App.Context.GetModule<MailModule>();
                module.ReplyToAllAsync();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnCompose(object sender, ExecutedRoutedEventArgs e) {
            try {
                var module = App.Context.GetModule<MailModule>();
                module.Compose();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private async void OnPrint(object sender, ExecutedRoutedEventArgs e) {
            try {
                await MessageViewer.PrintAsync();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnCanPrint(object sender, CanExecuteRoutedEventArgs e) {
            try {
                e.CanExecute = MessagesListView.SelectedValue != null;
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            try {
                DataContext = null;
                var module = App.Context.GetModule<MailModule>();
                module.SortOrderChanged -= OnSortOrderChanged;
                module.ItemSelectionRequested -= OnItemSelectionRequested;
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnSortOrderChanged(object sender, EventArgs e) {
            try {
                var module = App.Context.GetModule<MailModule>();
                var direction = module.IsSortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending;
                Sort(SortProperty, direction);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private async void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            try {
                if (!IsLoaded) {
                    return;
                }

                var tree = (TreeView)sender;
                var value = tree.SelectedValue;
                var module = App.Context.GetModule<MailModule>();

                module.SelectedMailbox = value as MailboxContext;

                RequeryRoutedCommands();

                var account = value as MailAccountContext;
                if (account == null)
                    return;

                if (NetworkInterface.GetIsNetworkAvailable()) {
                    await account.TakeOnlineAsync();
                }
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private static void RequeryRoutedCommands() {
            CommandManager.InvalidateRequerySuggested();
        }

        private async void OnAttachmentMouseDoubleClicked(object sender, MouseButtonEventArgs e) {
            try {
                if (!IsLoaded) {
                    return;
                }
                var view = (ListView)sender;
                var attachment = (MailAttachmentContext)view.SelectedValue;
                if (attachment == null) {
                    return;
                }
                await attachment.OpenAsync();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnInspect(object sender, ExecutedRoutedEventArgs e) {
            try {
                var message = e.Parameter as MailMessageContext;
                if (message == null) {
                    return;
                }

                var module = App.Context.GetModule<MailModule>();
                module.InspectMessage(message);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnMessageMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            try {
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
                Dispatcher.InvokeAsync(() => {
                    var module = App.Context.GetModule<MailModule>();
                    module.InspectMessage(message);
                });
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnSortPropertyButtonClicked(object sender, RoutedEventArgs e) {
            try {
                SortPropertyMenu.IsOpen = true;
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnSortPropertyMenuItemClicked(object sender, RoutedEventArgs e) {
            try {
                var item = (MenuItem)sender;
                var module = App.Context.GetModule<MailModule>();
                var direction = module.IsSortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending;
                Sort((SortProperty)item.DataContext, direction);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
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
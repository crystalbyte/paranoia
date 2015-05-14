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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Composition;
using System.Data.Entity;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Crystalbyte.Paranoia.Cryptography;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Data.SQLite;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.Themes;
using Crystalbyte.Paranoia.UI;
using Crystalbyte.Paranoia.UI.Commands;
using NLog;

#endregion

namespace Crystalbyte.Paranoia {
    [Export, Shared]
    public sealed class AppContext : NotificationObject {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private float _zoom;
        private bool _showOnlyUnseen;
        private string _queryString;
        private string _source;
        private bool _isPopupVisible;
        private bool _isSortAscending;
        private string _queryContactString;
        private bool _isAnimating;
        private MailboxContext _selectedMailbox;
        private OutboxContext _selectedOutbox;

        private readonly DispatcherTimer _outboxTimer;
        private readonly ICommand _resetZoomCommand;
        private readonly ICommand _restoreMessagesCommand;
        private readonly ICommand _markAsSeenCommand;
        private readonly ICommand _markAsNotSeenCommand;
        private readonly ICommand _markAsFlaggedCommand;
        private readonly ICommand _markAsNotFlaggedCommand;
        private readonly ICommand _blockContactsCommand;
        private readonly ICommand _unblockContactsCommand;
        private readonly ICommand _createAccountCommand;
        private readonly ICommand _deleteContactsCommand;
        private readonly ICommand _deleteMessagesCommand;
        private readonly DeferredObservableCollection<MailMessageContext> _messages;
        private readonly ObservableCollection<MailAccountContext> _accounts;
        private readonly ObservableCollection<MailContactContext> _contacts;
        private readonly ObservableCollection<NavigationContext> _navigationOptions;
        private IMessageSource _messageSource;

        #endregion

        #region Construction

        public AppContext() {
            _accounts = new ObservableCollection<MailAccountContext>();
            _accounts.CollectionChanged += OnAccountsCollectionChanged;

            _messages = new DeferredObservableCollection<MailMessageContext>();
            _messages.CollectionChanged += OnMessagesCollectionChanged;

            _navigationOptions = new ObservableCollection<NavigationContext> {
                new MailNavigationContext {
                    Title = Resources.MessagesTitle,
                    TargetUri = typeof (MailPage).ToPageUri(),
                    IsSelected = true
                },
                new NavigationContext {
                    Title = Resources.ContactsTitle, 
                    TargetUri = typeof (ContactsPage).ToPageUri(), 
                    Counter = 0
                }
            };

            _restoreMessagesCommand = new RestoreMessagesCommand(this);
            _markAsSeenCommand = new MarkAsSeenCommand(this);
            _blockContactsCommand = new BlockContactsCommand(this);
            _deleteMessagesCommand = new DeleteMessagesCommand(this);
            _markAsNotSeenCommand = new MarkAsNotSeenCommand(this);
            _deleteContactsCommand = new DeleteContactsCommand(this);
            _createAccountCommand = new RelayCommand(OnCreateAccount);
            _resetZoomCommand = new RelayCommand(p => Zoom = 0.0f);
            _unblockContactsCommand = new UnblockContactsCommand(this);
            _markAsFlaggedCommand = new MarkAsFlaggedCommand(this);
            _markAsNotFlaggedCommand = new MarkAsNotFlaggedCommand(this);

            NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;

            Observable.FromEventPattern(
                action => MessageSelectionChanged += action,
                action => MessageSelectionChanged -= action)
                .Throttle(TimeSpan.FromMilliseconds(100))
                .ObserveOn(new DispatcherSynchronizationContext(Application.Current.Dispatcher))
                .Subscribe(OnMessageSelectionReceived);

            Observable.FromEventPattern<QueryStringEventArgs>(
                action => QueryStringChanged += action,
                action => QueryStringChanged -= action)
                .Select(x => x.EventArgs)
                .Where(x => (x.Text.Length > 1 || String.IsNullOrEmpty(x.Text)))
                .Throttle(TimeSpan.FromMilliseconds(200))
                .Select(x => x.Text)
                .ObserveOn(new DispatcherSynchronizationContext(Application.Current.Dispatcher))
                .Subscribe(OnQueryReceived);

            Observable.FromEventPattern<QueryStringEventArgs>(
                action => ContactQueryStringChanged += action,
                action => ContactQueryStringChanged -= action)
                .Select(x => x.EventArgs)
                .Where(x => (x.Text.Length > 1 || String.IsNullOrEmpty(x.Text)))
                .Throttle(TimeSpan.FromMilliseconds(200))
                .Select(x => x.Text)
                .ObserveOn(new DispatcherSynchronizationContext(Application.Current.Dispatcher))
                .Subscribe(OnContactQueryReceived);

            _outboxTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _outboxTimer.Tick += OnOutboxTimerTick;

            _contacts = new ObservableCollection<MailContactContext>();
            _contacts.CollectionChanged += (sender, e) => RaisePropertyChanged(() => Contacts);

            _zoom = 0.0f;
            _showOnlyUnseen = false;
        }

        internal async void CountUnseenAsync() {
            var context = _navigationOptions.OfType<MailNavigationContext>().FirstOrDefault();
            if (context != null) {
                await context.RefreshAsync();
            }
        }

        private async void OnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e) {
            try {
                if (!e.IsAvailable)
                    return;

                await Application.Current.Dispatcher.InvokeAsync(async () => {
                    foreach (var account in Accounts) {
                        await account.TakeOnlineAsync();
                    }
                });
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private static async Task InvokeGroupedActionAsync(IEnumerable<MailMessageContext> messages,
            Action<ImapSession, IGrouping<MailboxContext, MailMessageContext>> action) {
            Application.Current.AssertUIThread();

            if (messages == null) {
                throw new ArgumentNullException("messages");
            }

            if (action == null) {
                throw new ArgumentNullException("action");
            }

            var tasks = messages
                .GroupBy(x => x.Mailbox.Account)
                .Select(x => Task.Run(async () => {
                    var account = x.Key;
                    var mailboxGroups = x.GroupBy(y => y.Mailbox);
                    try {
                        using (var connection = new ImapConnection { Security = account.ImapSecurity }) {
                            using (var auth = await connection.ConnectAsync(account.ImapHost, account.ImapPort)) {
                                using (var session = await auth.LoginAsync(account.ImapUsername, account.ImapPassword)) {
                                    foreach (var mailboxGroup in mailboxGroups) {
                                        action(session, mailboxGroup);
                                    }
                                }
                            }
                        }
                    } catch (Exception ex) {
                        Logger.Error(ex);
                    }
                })).ToArray();

            await Task.WhenAll(tasks);
        }

        internal async Task FlagMessagesAsync(MailMessageContext[] messages) {
            Application.Current.AssertUIThread();

            messages.ForEach(x => x.IsFlagged = true);

            try {
                await InvokeGroupedActionAsync(messages,
                    async (session, group) => {
                        var name = group.Key.Name;
                        var uids = group.Select(z => z.Uid).ToArray();

                        try {
                            var storage = FlagStoredMessagesAsync(uids);
                            var mailbox = await session.SelectAsync(name);
                            await mailbox.MarkAsFlaggedAsync(uids);

                            await storage;
                        } catch (Exception ex) {
                            Logger.Error(ex);
                        }
                    });
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private static Task FlagStoredMessagesAsync(IEnumerable<Int64> uids) {
            return Task.Run(async () => {
                using (var context = new DatabaseContext()) {
                    context.Connect();
                    context.EnableForeignKeys();

                    var entities = uids.Select(x => new MailMessage { Id = x });
                    foreach (var entity in entities) {
                        context.MailMessages.Attach(entity);
                        entity.Flags.Add(new MessageFlag { Value = MailMessageFlags.Seen });
                    }

                    await context.SaveChangesAsync(OptimisticConcurrencyStrategy.ClientWins);
                }
            });
        }

        internal async Task FlagMessagesAsSeenAsync(MailMessageContext[] messages) {
            Application.Current.AssertUIThread();

            messages.ForEach(x => x.IsSeen = true);

            try {
                await InvokeGroupedActionAsync(messages,
                    async (session, group) => {
                        var name = group.Key.Name;
                        var uids = group.Select(z => z.Uid).ToArray();
                        var ids = group.Select(z => z.Id).ToArray();

                        try {
                            var storage = FlagStoredMessagesAsSeenAsync(ids);
                            var mailbox = await session.SelectAsync(name);
                            await mailbox.MarkAsSeenAsync(uids);

                            await storage;
                        } catch (Exception ex) {
                            Logger.Error(ex);
                        }
                    });
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private static async Task FlagStoredMessagesAsSeenAsync(IEnumerable<Int64> ids) {
            Application.Current.AssertBackgroundThread();

            using (var context = new DatabaseContext()) {
                context.Connect();
                context.EnableForeignKeys();

                foreach (var id in ids) {
                    var entity = await context.MailMessages.FindAsync(id);
                    entity.Flags.Add(new MessageFlag { Value = MailMessageFlags.Seen });
                }

                await context.SaveChangesAsync(OptimisticConcurrencyStrategy.ClientWins);
            }
        }

        internal async Task DeleteMessagesAsync(MailMessageContext[] messages) {
            Application.Current.AssertUIThread();

            NotifyMessagesRemoved(messages);

            // Spool up a thread for each account.
            var tasks = messages
                .GroupBy(x => x.Mailbox.Account)
                .Select(x => Task.Run(async () => {
                    var account = x.Key;
                    var mailboxGroups = x.GroupBy(y => y.Mailbox);
                    try {
                        using (var connection = new ImapConnection { Security = account.ImapSecurity }) {
                            using (var auth = await connection.ConnectAsync(account.ImapHost, account.ImapPort)) {
                                using (var session = await auth.LoginAsync(account.ImapUsername, account.ImapPassword)) {
                                    foreach (var mailboxGroup in mailboxGroups) {
                                        var trashFolder = mailboxGroup.Key.Account.TrashMailboxName;
                                        var name = mailboxGroup.Key.Name;
                                        var uids = mailboxGroup.Select(z => z.Uid).ToArray();

                                        try {
                                            var storage = DropStoredMessagesAsync(mailboxGroup);

                                            var mailbox = await session.SelectAsync(name);
                                            if (mailboxGroup.Key.IsTrash) {
                                                await mailbox.DeleteMailsAsync(uids);
                                            } else {
                                                await mailbox.MoveMailsAsync(uids, trashFolder);
                                            }

                                            await storage;
                                        } catch (Exception ex) {
                                            Logger.Error(ex);
                                        }
                                    }
                                }
                            }
                        }
                    } catch (Exception ex) {
                        Logger.Error(ex);
                    }
                })).ToArray();

            await Task.WhenAll(tasks);
        }

        private static async Task DropStoredMessagesAsync(IGrouping<MailboxContext, MailMessageContext> mailboxGroup) {
            Application.Current.AssertBackgroundThread();

            using (var context = new DatabaseContext()) {
                context.Connect();
                context.EnableForeignKeys();

                foreach (var message in mailboxGroup) {
                    var id = message.Id;
                    using (var transaction = context.Database.BeginTransaction()) {
                        try {

                            var model = new MailMessage {
                                Id = message.Id,
                                MailboxId = mailboxGroup.Key.Id
                            };

                            context.MailMessages.Attach(model);
                            context.MailMessages.Remove(model);

                            // TODO: Extract column names from expression attributes.
                            // The content table is a virtual table and cannot be altered using EF.
                            context.Database.ExecuteSqlCommand(
                                TransactionalBehavior.DoNotEnsureTransaction,
                                "DELETE FROM mail_content WHERE message_id = @id;",
                                new SQLiteParameter("@id", id));

                            await context.SaveChangesAsync(OptimisticConcurrencyStrategy.ClientWins);

                            transaction.Commit();
                        } catch (Exception ex) {
                            transaction.Rollback();
                            Logger.Error(ex);
                        }
                    }
                }
            }
        }

        private async void OnContactQueryReceived(string query) {
            try {
                await FilterContactsAsync(query);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnMessagesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            try {
                RaisePropertyChanged(() => Messages);
                RaisePropertyChanged(() => MessageCount);
                RaisePropertyChanged(() => IsMessageOrSmtpRequestSelected);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        public object Alphabet {
            get {
                return Enumerable.Range(65, 26)
                    .Select(Convert.ToChar).ToArray();
            }
        }

        internal async Task FilterContactsAsync(string query) {
            Application.Current.AssertUIThread();

            throw new NotImplementedException();
            _contacts.Clear();

            if (String.IsNullOrWhiteSpace(query)) {
                await Task.Run(async () => await LoadContactsAsync());
                return;
            }

            var contacts = await Task.Run(async () => {
                using (var database = new DatabaseContext()) {
                    return await database.MailContacts
                        .Where(x => x.Name.Contains(query)
                                    || x.Address.Contains(query))
                        .ToArrayAsync();
                }
            });

            await Application.Current.Dispatcher.InvokeAsync(() => _contacts
                .AddRange(contacts.Select(x => new MailContactContext(x))));
        }

        internal async Task LoadContactsAsync() {
            Application.Current.AssertUIThread();

            var contacts = await Task.Run(() => {
                using (var database = new DatabaseContext()) {
                    return database.MailContacts.ToArrayAsync();
                }
            });

            var contexts = contacts.Select(x => new MailContactContext(x));
            _contacts.AddRange(contexts);
        }

        /// <summary>
        ///     Queries the message source.
        /// </summary>
        /// <returns>Returns a task object.</returns>
        private async Task QueryMessageSource() {
            Application.Current.AssertUIThread();

            var source = MessageSource;
            if (source == null) {
                return;
            }

            try {
                _messages.Clear();

                source.BeginQuery();
                var messages = await Task.Run(() => source.GetMessagesAsync());
                source.FinishQuery();

                // User might have switched to a different source by now.
                if (MessageSource != source) {
                    return;
                }

                _messages.DeferNotifications = true;
                _messages.AddRange(messages);
                _messages.DeferNotifications = false;
                _messages.NotifyCollectionChanged();

                OnItemSelectionRequested(new ItemSelectionRequestedEventArgs(SelectionPosition.First));
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        #endregion

        #region Public Events

        public event EventHandler OutboxSelectionChanged;

        private async void OnOutboxSelectionChanged() {
            try {
                var handler = OutboxSelectionChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);

                if (SelectedOutbox == null) {
                    return;
                }

                Clear();
                await RefreshViewForSelectedOutbox();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void Clear() {
            _messages.Clear();
            _accounts.ForEach(x => x.Outbox.ClearSmtpRequests());
            Source = null;
        }

        public event EventHandler MailboxSelectionChanged;

        private void OnMailboxSelectionChanged() {
            try {
                var handler = MailboxSelectionChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);

                if (SelectedMailbox == null) {
                    return;
                }

                Clear();

                MessageSource = SelectedMailbox;
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private async Task RefreshViewForSelectedOutbox() {
            Application.Current.AssertUIThread();

            await RequestOutboxContentAsync(SelectedOutbox);
        }

        internal event EventHandler Initialized;

        private void OnInitialized() {
            var handler = Initialized;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        internal event EventHandler FlyoutClosing;

        private void OnFlyoutClosing() {
            var handler = FlyoutClosing;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        internal event EventHandler FlyoutClosed;

        internal void OnFlyoutClosed() {
            var handler = FlyoutClosed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public event EventHandler<ItemSelectionRequestedEventArgs> ItemSelectionRequested;

        private void OnItemSelectionRequested(ItemSelectionRequestedEventArgs e) {
            var handler = ItemSelectionRequested;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler SortOrderChanged;

        private void OnSortOrderChanged() {
            var handler = SortOrderChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public event EventHandler FlyoutCloseRequested;

        private void OnFlyoutCloseRequested() {
            var handler = FlyoutCloseRequested;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        internal event EventHandler<NavigationRequestedEventArgs> ModalNavigationRequested;

        private void OnModalNavigationRequested(NavigationRequestedEventArgs e) {
            var handler = ModalNavigationRequested;
            if (handler != null)
                handler(this, e);
        }

        internal event EventHandler<NavigationRequestedEventArgs> FlyoutNavigationRequested;

        private void OnFlyoutNavigationRequested(NavigationRequestedEventArgs e) {
            var handler = FlyoutNavigationRequested;
            if (handler != null) {
                handler(this, e);
            }
        }

        internal event EventHandler MessageSelectionChanged;

        internal void OnMessageSelectionChanged() {
            var handler = MessageSelectionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);

            RaisePropertyChanged(() => SelectedMessage);
            RaisePropertyChanged(() => SelectedMessages);
            RaisePropertyChanged(() => IsMessageSelected);
            RaisePropertyChanged(() => IsMessageOrSmtpRequestSelected);
        }

        internal event EventHandler ContactSelectionChanged;

        internal void OnContactSelectionChanged() {
            var handler = ContactSelectionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);

            RaisePropertyChanged(() => SelectedContact);
            RaisePropertyChanged(() => SelectedContacts);
        }

        internal event EventHandler<QueryStringEventArgs> QueryStringChanged;

        private void OnQueryStringChanged(QueryStringEventArgs e) {
            var handler = QueryStringChanged;
            if (handler != null)
                handler(this, e);
        }

        internal event EventHandler<QueryStringEventArgs> ContactQueryStringChanged;


        private void OnContactQueryStringChanged(QueryStringEventArgs e) {
            var handler = ContactQueryStringChanged;
            if (handler != null)
                handler(this, e);
        }

        private static async Task RequestOutboxContentAsync(OutboxContext outbox) {
            await outbox.LoadCompositionsAsync();
        }

        #endregion

        #region Property Declarations

        [ImportMany]
        public IEnumerable<Theme> Themes { get; set; }

        public IEnumerable<NavigationContext> NavigationOptions {
            get { return _navigationOptions; }
        }

        public IEnumerable<MailContactContext> Contacts {
            get { return _contacts; }
        }

        internal void ConfigureAccount(MailAccountContext account) {
            if (account == null) {
                throw new ArgumentNullException("account");
            }

            NavigationArguments.Push(account);
            var uri = typeof(AccountPropertyFlyoutPage).ToPageUri();
            OnFlyoutNavigationRequested(new NavigationRequestedEventArgs(uri));
        }

        public bool IsPopupVisible {
            get { return _isPopupVisible; }
            set {
                if (_isPopupVisible == value) {
                    return;
                }
                _isPopupVisible = value;
                RaisePropertyChanged(() => IsPopupVisible);
            }
        }

        public bool IsOutboxSelected {
            get { return SelectedOutbox != null; }
        }

        public MailboxContext SelectedMailbox {
            get { return _selectedMailbox; }
            set {
                if (_selectedMailbox == value) {
                    return;
                }

                _selectedMailbox = value;
                RaisePropertyChanged(() => SelectedMailbox);
                RaisePropertyChanged(() => IsOutboxSelected);
                OnMailboxSelectionChanged();
            }
        }

        public OutboxContext SelectedOutbox {
            get { return _selectedOutbox; }
            set {
                if (_selectedOutbox == value) {
                    return;
                }

                _selectedOutbox = value;
                RaisePropertyChanged(() => SelectedOutbox);
                RaisePropertyChanged(() => IsOutboxSelected);
                OnOutboxSelectionChanged();
            }
        }

        public string Source {
            get { return _source; }
            set {
                if (_source == value) {
                    return;
                }
                _source = value;
                RaisePropertyChanged(() => Source);
            }
        }

        internal async Task PublishAccountAsync(MailAccountContext account) {
            _accounts.Add(account);
            await account.TakeOnlineAsync();
        }

        public IEnumerable<MailAccountContext> Accounts {
            get { return _accounts; }
        }

        public bool IsMessageSelected {
            get { return SelectedMessage != null; }
        }

        public bool IsAnimating {
            get { return _isAnimating; }
            set {
                if (_isAnimating == value) {
                    return;
                }
                _isAnimating = value;
                RaisePropertyChanged(() => IsAnimating);
            }
        }

        public bool ShowOnlyUnseen {
            get { return _showOnlyUnseen; }
            set {
                if (_showOnlyUnseen == value) {
                    return;
                }
                _showOnlyUnseen = value;
                RaisePropertyChanged(() => ShowOnlyUnseen);
                OnMessageFilterChanged();
            }
        }

        private async void OnMessageFilterChanged() {
            try {
                if (MessageSource != null) {
                    await QueryMessageSource();
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        public string QueryString {
            get { return _queryString; }
            set {
                if (_queryString == value) {
                    return;
                }
                _queryString = value;
                RaisePropertyChanged(() => QueryString);
                OnQueryStringChanged(new QueryStringEventArgs(value));
            }
        }

        public IMessageSource MessageSource {
            get { return _messageSource; }
            set {
                if (_messageSource == value) {
                    return;
                }
                _messageSource = value;
                RaisePropertyChanged(() => MessageSource);
                OnMessageSourceChanged();
            }
        }

        public string ContactQueryString {
            get { return _queryContactString; }
            set {
                if (_queryContactString == value) {
                    return;
                }
                _queryContactString = value;
                RaisePropertyChanged(() => ContactQueryString);
                OnContactQueryStringChanged(new QueryStringEventArgs(value));
            }
        }

        public IEnumerable<MailMessageContext> Messages {
            get { return _messages; }
        }

        public int MessageCount {
            get { return _messages.Count; }
        }

        public bool IsSortAscending {
            get { return _isSortAscending; }
            set {
                if (_isSortAscending == value) {
                    return;
                }
                _isSortAscending = value;
                RaisePropertyChanged(() => IsSortAscending);
                OnSortOrderChanged();
            }
        }

        public float Zoom {
            get { return _zoom; }
            set {
                if (Math.Abs(_zoom - value) < Single.Epsilon) {
                    return;
                }
                _zoom = value;
                RaisePropertyChanged(() => Zoom);
            }
        }

        public ICommand ResetZoomCommand {
            get { return _resetZoomCommand; }
        }

        public ICommand BlockContactsCommand {
            get { return _blockContactsCommand; }
        }

        public ICommand UnblockContactsCommand {
            get { return _unblockContactsCommand; }
        }

        public ICommand RestoreMessagesCommand {
            get { return _restoreMessagesCommand; }
        }

        public ICommand DeleteContactsCommand {
            get { return _deleteContactsCommand; }
        }

        public ICommand DeleteMessagesCommand {
            get { return _deleteMessagesCommand; }
        }

        public ICommand MarkAsSeenCommand {
            get { return _markAsSeenCommand; }
        }

        public ICommand MarkAsNotSeenCommand {
            get { return _markAsNotSeenCommand; }
        }

        public ICommand MarkAsNotFlaggedCommand {
            get { return _markAsNotFlaggedCommand; }
        }

        public ICommand MarkAsFlaggedCommand {
            get { return _markAsFlaggedCommand; }
        }

        public ICommand CreateAccountCommand {
            get { return _createAccountCommand; }
        }

        public IEnumerable<MailMessageContext> SelectedMessages {
            get { return _messages.Where(x => x.IsSelected).ToArray(); }
        }

        public IEnumerable<MailContactContext> SelectedContacts {
            get { return _contacts.Where(x => x.IsSelected).ToArray(); }
        }

        public MailContactContext SelectedContact {
            get {
                return SelectedContacts == null
                    ? null
                    : SelectedContacts.FirstOrDefault();
            }
        }

        public MailMessageContext SelectedMessage {
            get {
                return SelectedMessages == null
                    ? null
                    : SelectedMessages.FirstOrDefault();
            }
        }

        #endregion

        private void OnQueryReceived(string text) {
            try {
                if (string.IsNullOrEmpty(text)) {
                    MessageSource = SelectedMailbox;
                } else {
                    MessageSource = new MessageQuery(text);
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnAccountsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            try {
                RaisePropertyChanged(() => Accounts);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private async void OnMessageSelectionReceived(EventPattern<object> obj) {
            Application.Current.AssertUIThread();

            try {
                var message = SelectedMessages.FirstOrDefault();
                if (message == null) {
                    return;
                }

                try {
                    await ViewMessageAsync(message);
                } catch (Exception ex) {
                    Logger.Error(ex);
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        internal async Task ViewMessageAsync(MailMessageContext message) {
            Application.Current.AssertUIThread();

            try {
                await Task.Run(async () => {
                    var hasMime = await message.GetIsMimeStoredAsync();
                    if (!hasMime) {
                        await message.FetchAndDecryptAsync();
                    }
                });

                if (!message.IsInitialized) {
                    await message.InitDetailsAsync();
                }

                Source = string.Format(message.IsExternalContentAllowed
                    ? "message:///{0}?blockExternals=false"
                    : "message:///{0}", message.Id);

            } catch (MessageDecryptionFailedException ex) {
                // TODO: Notify user key is missing.
                Logger.Error(ex);
            } catch (MissingKeyException ex) {
                // TODO: Notify user key is missing.
                Logger.Error(ex);
            } catch (SignetMissingOrCorruptException ex) {
                // TODO: Notify user signet is missing or corrupt.
                Logger.Error(ex);
            } catch (MissingContactException ex) {
                // TODO: Notify user the contact is not listed in the database.
                Logger.Error(ex);
            } catch (Exception ex) {
                // TODO: General exception handling.
                Logger.Error(ex);
            }
        }

        internal Task MarkSelectionAsSeenAsync() {
            var messages = SelectedMessages.ToArray();

            return Task.Run(() => {
                var tasks = messages
                    .Where(x => x.IsNotSeen)
                    .GroupBy(x => x.Mailbox)
                    .Select(x => x.Key.MarkAsSeenAsync(x.ToArray()));

                Task.WhenAll(tasks);
            });
        }

        internal Task MarkSelectionAsNotSeenAsync() {
            var messages = SelectedMessages.ToArray();

            return Task.Run(() => {
                var tasks = messages
                    .Where(x => x.IsSeen)
                    .GroupBy(x => x.Mailbox)
                    .Select(x => x.Key.MarkAsNotSeenAsync(x.ToArray()));

                Task.WhenAll(tasks);
            });
        }

        internal void ClosePopup() {
            OnModalNavigationRequested(new NavigationRequestedEventArgs(null));
            IsPopupVisible = false;
        }

        private static Task<bool> CheckKeyPairAsync() {
            return Task.Run(() => {
                using (var context = new DatabaseContext()) {
                    return context.KeyPairs.Any();
                }
            });
        }

        private static Task GenerateKeyPairAsync() {
            return Task.Run(() => {
                var crypto = new PublicKeyCrypto();
                var pair = new KeyPair {
                    PublicKey = crypto.PublicKey,
                    PrivateKey = crypto.PrivateKey,
                    Date = DateTime.Now
                };

                using (var context = new DatabaseContext()) {
                    context.KeyPairs.Add(pair);
                    context.SaveChanges();
                }
            });
        }

        public async Task RunAsync() {
            Application.Current.AssertUIThread();

            try {
                var context = NavigationOptions.OfType<MailNavigationContext>().First();
                var refresh = context.RefreshAsync();
                var loadContacts = LoadContactsAsync();

                if (!await CheckKeyPairAsync()) {
                    await GenerateKeyPairAsync();
                }

                await LoadAccountsAsync();
                
                var primary = Accounts.OrderByDescending(x => x.IsDefaultTime).FirstOrDefault();
                if (primary != null) {
                    primary.IsExpanded = true;

                    var inbox = primary.GetInbox();
                    if (inbox != null) {
                        inbox.IsSelected = true;
                    }
                }

                var online = Accounts.Select(x => x.TakeOnlineAsync()).ToArray();
                await Task.WhenAll(online.Concat(new[] { loadContacts, refresh }));

                _outboxTimer.Start();

                OnInitialized();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private async Task LoadAccountsAsync() {
            Application.Current.AssertUIThread();

            try {
                var accounts = await Task.Run(() => {
                    using (var context = new DatabaseContext()) {
                        return context.MailAccounts.ToArrayAsync();
                    }
                });

                var contexts = accounts.Select(x => new MailAccountContext(x)).ToArray();
                var queries = contexts.SelectMany(x => new[] {
                    x.LoadMailboxesAsync(), 
                    x.Outbox.CountSmtpRequestsAsync()
                });

                _accounts.AddRange(contexts);

                await Task.WhenAll(queries);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        internal void CreateMailbox(IMailboxCreator parent) {
            NavigationArguments.Push(parent);

            var uri = typeof(CreateMailboxModalPage).ToPageUri();
            OnModalNavigationRequested(new NavigationRequestedEventArgs(uri));
            IsPopupVisible = true;
        }

        private async void OnMessageSourceChanged() {
            try {
                if (MessageSource != null) {
                    await QueryMessageSource();
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnCreateAccount(object obj) {
            try {
                var account = new MailAccountContext(new MailAccount());
                NavigationArguments.Push(account);

                var uri = typeof(CreateAccountStartFlyoutPage).ToPageUri();
                OnFlyoutNavigationRequested(new NavigationRequestedEventArgs(uri));
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        internal void CloseFlyout() {
            OnFlyoutClosing();
            OnFlyoutCloseRequested();
            OnFlyoutClosed();
        }

        private async void OnOutboxTimerTick(object sender, EventArgs e) {
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return;
            }

            try {
                await ProcessOutgoingMessagesAsync();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        internal async Task RestoreSelectedMessagesAsync() {
            try {
                var messages = SelectedMessages.ToArray();
                var accountGroups = messages.GroupBy(x => x.Mailbox.Account).ToArray();
                foreach (var accountGroup in accountGroups) {
                    var trash = accountGroup.Key.GetTrashMailbox();
                    if (trash == null) {
                        throw new InvalidOperationException("Trash must not be null.");
                    }

                    var mailboxGroups = accountGroup.GroupBy(x => x.Mailbox).ToArray();
                    foreach (var mailboxGroup in mailboxGroups) {
                        var groupedMessages = mailboxGroup.ToArray();
                        await mailboxGroup.Key.RestoreMessagesAsync(groupedMessages);
                    }
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private Task ProcessOutgoingMessagesAsync() {
            var tasks = Accounts.Select(x => x.Outbox.SendCompositionsAsync());
            return Task.WhenAll(tasks);
        }

        internal async Task NotifyOutboxNotEmpty() {
            foreach (var account in Accounts) {
                await account.Outbox.CountSmtpRequestsAsync();
            }
            await ProcessOutgoingMessagesAsync();
        }

        public bool IsMessageOrSmtpRequestSelected {
            get {
                return SelectedMessage != null
                       || (SelectedOutbox != null && SelectedOutbox.SelectedComposition != null);
            }
        }

        internal void NotifySmtpRequestChanged() {
            RaisePropertyChanged(() => IsMessageOrSmtpRequestSelected);
        }

        internal void NotifyContactsAdded(ICollection<MailContactContext> contacts) {
            Application.Current.AssertUIThread();

            _contacts.AddRange(contacts);
        }

        internal void NotifyMessagesAdded(ICollection<MailMessageContext> messages) {
            foreach (var message in messages.Where(message => message.Mailbox.IsSelected)) {
                _messages.Add(message);
            }

            RaisePropertyChanged(() => Messages);
        }

        internal Task ReplyAsync() {
            return ReplyAsync(SelectedMessage);
        }

        internal async Task ReplyAsync(MailMessageContext message) {
            var id = message.Id.ToString(CultureInfo.InvariantCulture);
            var owner = Application.Current.MainWindow;
            var window = new CompositionWindow();
            await window.PrepareAsReplyAsync(new Dictionary<string, string> {
                {"id", id}
            });
            window.MimicOwnership(owner);

            if (owner.WindowState == WindowState.Maximized) {
                window.WindowState = WindowState.Maximized;
            }

            window.Show();
        }

        internal async Task ReplyAsync(FileMessageContext file) {
            var path = Uri.EscapeDataString(file.FullName);
            var owner = Application.Current.MainWindow;
            var window = new CompositionWindow();
            await window.PrepareAsReplyAsync(new Dictionary<string, string> {
                {"path", path}
            });

            window.MimicOwnership(owner);

            if (owner.WindowState == WindowState.Maximized) {
                window.WindowState = WindowState.Maximized;
            }

            window.Show();
        }

        internal Task ForwardAsync() {
            return ForwardAsync(SelectedMessage);
        }

        internal async Task ForwardAsync(MailMessageContext message) {
            var id = message.Id.ToString(CultureInfo.InvariantCulture);
            var owner = Application.Current.MainWindow;
            var window = new CompositionWindow();
            await window.PrepareAsForwardAsync(new Dictionary<string, string>
            {
                {"id", id}
            });
            window.MimicOwnership(owner);

            if (owner.WindowState == WindowState.Maximized) {
                window.WindowState = WindowState.Maximized;
            }

            window.Show();
        }

        internal async Task ForwardAsync(FileMessageContext file) {
            var path = Uri.EscapeDataString(file.FullName);
            var owner = Application.Current.MainWindow;
            var window = new CompositionWindow();
            await window.PrepareAsForwardAsync(new Dictionary<string, string>
            {
                {"path", path}
            });
            window.MimicOwnership(owner);

            if (owner.WindowState == WindowState.Maximized) {
                window.WindowState = WindowState.Maximized;
            }

            window.Show();
        }

        internal Task ReplyToAllAsync() {
            return ReplyToAllAsync(SelectedMessage);
        }

        internal async Task ReplyToAllAsync(MailMessageContext message) {
            var id = message.Id.ToString(CultureInfo.InvariantCulture);
            var owner = Application.Current.MainWindow;
            var window = new CompositionWindow();
            await window.PrepareAsReplyAllAsync(new Dictionary<string, string>
            {
                {"id", id}
            });
            window.MimicOwnership(owner);

            if (owner.WindowState == WindowState.Maximized) {
                window.WindowState = WindowState.Maximized;
            }

            window.Show();
        }

        internal async Task ReplyToAllAsync(FileMessageContext message) {
            var path = Uri.EscapeDataString(message.FullName);

            var owner = Application.Current.MainWindow;
            var window = new CompositionWindow();
            await window.PrepareAsReplyAllAsync(new Dictionary<string, string>
            {
                {"path", path}
            });
            window.MimicOwnership(owner);

            if (owner.WindowState == WindowState.Maximized) {
                window.WindowState = WindowState.Maximized;
            }

            window.Show();
        }


        internal void Compose() {
            var owner = Application.Current.MainWindow;
            var window = new CompositionWindow();
            window.MimicOwnership(Application.Current.MainWindow);
            window.PrepareAsNew();

            if (owner.WindowState == WindowState.Maximized) {
                window.WindowState = WindowState.Maximized;
            }

            window.Show();
        }

        internal void InspectMessage(FileInfo file) {
            var owner = Application.Current.MainWindow;
            var inspector = new InspectionWindow();
            inspector.MimicOwnership(owner);

            if (owner.WindowState == WindowState.Maximized) {
                inspector.WindowState = WindowState.Maximized;
            }

            inspector.InitWithFile(new FileMessageContext(file));
            inspector.Show();
        }

        internal void InspectMessage(MailMessageContext message) {
            var owner = Application.Current.MainWindow;
            var inspector = new InspectionWindow();
            inspector.MimicOwnership(owner);

            if (owner.WindowState == WindowState.Maximized) {
                inspector.WindowState = WindowState.Maximized;
            }

            inspector.InitWithMessage(message);
            inspector.Show();
        }

        internal void ShowMessage(MailMessageContext message) {
            if (!message.Mailbox.IsSelected) {
                message.Mailbox.IsSelected = true;
            }

            _messages.ForEach(x => x.IsSelected = false);
            message.IsSelected = true;
        }

        internal void NotifyMessagesRemoved(IEnumerable<long> ids) {
            foreach (
                var message in
                    ids.Select(id => _messages.FirstOrDefault(x => x.Id == id)).Where(message => message != null)) {
                _messages.Remove(message);
            }
        }

        internal void NotifyMessagesRemoved(IEnumerable<MailMessageContext> messages) {
            var collection = messages as MailMessageContext[] ?? messages.ToArray();
            collection.ForEach(x => _messages.Remove(x));

            OnItemSelectionRequested(new ItemSelectionRequestedEventArgs(SelectionPosition.Next, collection));
        }

        internal async Task DeleteSelectedContactsAsync() {
            using (var database = new DatabaseContext()) {
                var contacts = _contacts.Select(x => new MailContact {
                    Id = x.Id
                }).ToArray();

                database.MailContacts.RemoveRange(contacts);

                foreach (var contact in contacts) {
                    var c = contact;
                    var keys = await database.PublicKeys
                        .Where(x => x.ContactId == c.Id)
                        .ToArrayAsync();
                    database.PublicKeys.RemoveRange(keys);
                }

                await database.SaveChangesAsync();
            }
        }

        internal void NotifyAccountDeleted(MailAccountContext account) {
            _accounts.Remove(account);
            RaisePropertyChanged(() => Accounts);
        }

        public Task MarkSelectionAsNotFlaggedAsync() {
            var messages = SelectedMessages.ToArray();

            return Task.Run(() => {
                var tasks = messages
                    .Where(x => x.IsSeen)
                    .GroupBy(x => x.Mailbox)
                    .Select(x => x.Key.MarkAsNotFlaggedAsync(x.ToArray()));

                Task.WhenAll(tasks);
            });
        }

        internal void NotifySeenStatesChanged(IDictionary<long, MailMessage> messages) {
            foreach (var message in
                from message in _messages
                let hasKey = messages.ContainsKey(message.Id)
                where hasKey select message) {
                message.IsSeen = messages[message.Id].Flags.Any(x => x.Value == MailMessageFlags.Seen);
            }
        }
    }
}
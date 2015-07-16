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
using System.Text.RegularExpressions;
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
using MailMessage = Crystalbyte.Paranoia.Data.MailMessage;

#endregion

namespace Crystalbyte.Paranoia {
    [Export, Shared]
    public sealed class AppContext : NotificationObject {

        #region Private Fields

        private string _source;
        private bool _isAnimating;
        private bool _isPopupVisible;
        private bool _showOnlyUnseen;
        private bool _isSortAscending;

        private IMessageSource _messageSource;

        private MailboxContext _selectedMailbox;
        private MailContactContext _activeContact;

        private readonly DispatcherTimer _outboxTimer;
        private readonly ICommand _flagMessagesCommand;
        private readonly ICommand _createAccountCommand;
        private readonly ICommand _unflagMessagesCommand;
        private readonly ICommand _deleteContactsCommand;
        private readonly ICommand _deleteMessagesCommand;
        private readonly ICommand _restoreMessagesCommand;
        private readonly ICommand _markMessagesAsSeenCommand;
        private readonly ICommand _markMessagesAsUnseenCommand;
        private readonly DeferredObservableCollection<MailMessageContext> _messages;
        private readonly DeferredObservableCollection<MailContactContext> _contacts;
        private readonly ObservableCollection<MailAccountContext> _accounts;
        private readonly ObservableCollection<NavigationContext> _navigationOptions;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public AppContext() {
            _accounts = new DeferredObservableCollection<MailAccountContext>();
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
            _markMessagesAsSeenCommand = new FlagMessagesAsSeenCommand(this);
            _deleteMessagesCommand = new DeleteMessagesCommand(this);
            _markMessagesAsUnseenCommand = new FlagMessagesAsUnseenCommand(this);
            _deleteContactsCommand = new DeleteContactsCommand(this);
            _createAccountCommand = new RelayCommand(OnCreateAccount);
            _flagMessagesCommand = new FlagMessagesCommand(this);
            _unflagMessagesCommand = new UnflagMessagesCommand(this);

            _outboxTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _outboxTimer.Tick += OnOutboxTimerTick;

            _contacts = new DeferredObservableCollection<MailContactContext>();
            _contacts.CollectionChanged += (sender, e) => RaisePropertyChanged(() => Contacts);

            _showOnlyUnseen = false;
        }

        internal async void CountUnseenAsync() {
            var context = _navigationOptions.OfType<MailNavigationContext>().FirstOrDefault();
            if (context != null) {
                await context.CountGlobalUnseenAsync();
            }
        }

        private static async Task InvokeGroupedActionAsync(IEnumerable<MailMessageContext> messages,
            Func<ImapSession, IGrouping<MailboxContext, MailMessageContext>, Task> action) {
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
                                        await action(session, mailboxGroup);
                                    }
                                }
                            }
                        }
                    } catch (Exception ex) {
                        Logger.ErrorException(ex.Message, ex);
                    }
                })).ToArray();

            await Task.WhenAll(tasks);
        }

        internal async Task MarkMessagesAsFlaggedAsync(MailMessageContext[] messages) {
            Logger.Enter();

            Application.Current.AssertUIThread();

            messages.ForEach(x => x.IsFlagged = true);

            try {
                await InvokeGroupedActionAsync(messages,
                    async (session, group) => {
                        var name = group.Key.Name;
                        var uids = group.Select(z => z.Uid).ToArray();
                        var ids = group.Select(z => z.Id).ToArray();

                        try {
                            var storage = SaveFlagsToStoreAsync(ids, MailMessageFlags.Flagged);
                            var mailbox = await session.SelectAsync(name);
                            await mailbox.MarkAsFlaggedAsync(uids);

                            await storage;
                        } catch (Exception ex) {
                            Logger.ErrorException(ex.Message, ex);
                        }
                    });
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                Logger.Exit();
            }
        }

        internal async Task MarkMessagesAsUnflaggedAsync(MailMessageContext[] messages) {
            Logger.Enter();

            Application.Current.AssertUIThread();

            messages.ForEach(x => x.IsFlagged = false);

            try {
                await InvokeGroupedActionAsync(messages,
                    async (session, group) => {
                        var name = group.Key.Name;
                        var uids = group.Select(z => z.Uid).ToArray();
                        var ids = group.Select(z => z.Id).ToArray();

                        try {
                            var storage = DeleteFlagsFromStoreAsync(ids, MailMessageFlags.Flagged);
                            var mailbox = await session.SelectAsync(name);
                            await mailbox.MarkAsNotFlaggedAsync(uids);

                            await storage;
                        } catch (Exception ex) {
                            Logger.ErrorException(ex.Message, ex);
                        }
                    });
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                Logger.Exit();
            }
        }

        private static async Task DeleteFlagsFromStoreAsync(IEnumerable<Int64> ids, string value) {
            Logger.Enter();

            Application.Current.AssertBackgroundThread();

            try {
                using (var context = new DatabaseContext()) {
                    var flags = await context.Set<MailMessageFlag>()
                        .Where(x => ids.Contains(x.MessageId) && x.Value == value)
                        .ToArrayAsync();

                    context.Set<MailMessageFlag>().RemoveRange(flags);

                    await context.SaveChangesAsync(OptimisticConcurrencyStrategy.ClientWins);
                }
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                Logger.Exit();
            }
        }

        internal async Task MarkMessagesAsUnseenAsync(MailMessageContext[] messages) {
            Logger.Enter();

            Application.Current.AssertUIThread();

            var candidates = messages.Where(x => x.IsSeen).ToArray();
            if (candidates.Length == 0) {
                return;
            }

            candidates.ForEach(x => x.IsSeen = false);

            try {
                await InvokeGroupedActionAsync(candidates,
                    async (session, group) => {
                        var name = group.Key.Name;
                        var ids = group.Select(z => z.Id).ToArray();
                        var uids = group.Select(z => z.Uid).ToArray();

                        try {
                            var storage = DeleteFlagsFromStoreAsync(ids, MailMessageFlags.Seen);
                            var mailbox = await session.SelectAsync(name);
                            await mailbox.MarkAsSeenAsync(uids);

                            await storage;
                        } catch (Exception ex) {
                            Logger.ErrorException(ex.Message, ex);
                        }
                    });

                var countUnseenTasks = messages.GroupBy(x => x.Mailbox).Select(x => x.Key.CountMessagesAsync());
                await Task.WhenAll(countUnseenTasks);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                Logger.Exit();
            }
        }

        internal async Task MarkMessagesAsSeenAsync(MailMessageContext[] messages) {
            Logger.Enter();

            Application.Current.AssertUIThread();

            var candidates = messages.Where(x => !x.IsSeen).ToArray();
            if (candidates.Length == 0) {
                return;
            }

            candidates.ForEach(x => x.IsSeen = true);

            try {
                await InvokeGroupedActionAsync(candidates,
                    async (session, group) => {
                        var name = group.Key.Name;
                        var ids = group.Select(z => z.Id).ToArray();
                        var uids = group.Select(z => z.Uid).ToArray();

                        try {
                            var storage = SaveFlagsToStoreAsync(ids, MailMessageFlags.Seen);
                            var mailbox = await session.SelectAsync(name);
                            await mailbox.MarkAsSeenAsync(uids);
                            await storage;
                        } catch (Exception ex) {
                            Logger.ErrorException(ex.Message, ex);
                        }
                    });

                var countUnseenTasks = messages.GroupBy(x => x.Mailbox).Select(x => x.Key.CountMessagesAsync());
                await Task.WhenAll(countUnseenTasks);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                Logger.Exit();
            }
        }

        private static async Task SaveFlagsToStoreAsync(IEnumerable<Int64> ids, string value) {
            Logger.Enter();

            Application.Current.AssertBackgroundThread();

            try {
                using (var context = new DatabaseContext()) {
                    var flags = ids.Select(x => new MailMessageFlag {
                        MessageId = x
                    });

                    foreach (var flag in flags) {
                        context.Set<MailMessageFlag>().Add(flag);
                        flag.Value = value;
                    }

                    await context.SaveChangesAsync(OptimisticConcurrencyStrategy.ClientWins);
                }
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                Logger.Exit();
            }
        }

        internal async Task DeleteMessagesAsync(MailMessageContext[] messages) {
            Logger.Enter();

            Application.Current.AssertUIThread();

            NotifyMessagesRemoved(messages);

            try {
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
                                                var storage = DeleteStoredMessagesAsync(mailboxGroup);

                                                var mailbox = await session.SelectAsync(name);
                                                if (mailboxGroup.Key.IsTrash) {
                                                    await mailbox.DeleteMailsAsync(uids);
                                                } else {
                                                    await mailbox.MoveMailsAsync(uids, trashFolder);
                                                }

                                                await storage;
                                            } catch (Exception ex) {
                                                Logger.ErrorException(ex.Message, ex);
                                            }
                                        }
                                    }
                                }
                            }
                        } catch (Exception ex) {
                            Logger.ErrorException(ex.Message, ex);
                        }
                    })).ToArray();

                await Task.WhenAll(tasks);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                Logger.Exit();
            }
        }

        private void NotifyMessagesRemoved(IEnumerable<MailMessageContext> messages) {
            Application.Current.AssertUIThread();

            _messages.DeferNotifications = true;
            foreach (var message in messages) {
                _messages.Remove(message);
            }

            _messages.DeferNotifications = false;
            _messages.NotifyCollectionChanged();
        }

        private static async Task DeleteStoredMessagesAsync(IGrouping<MailboxContext, MailMessageContext> mailboxGroup) {
            Logger.Enter();

            Application.Current.AssertBackgroundThread();

            try {
                using (var context = new DatabaseContext()) {
                    await context.OpenAsync();
                    await context.EnableForeignKeysAsync();
                    throw new NotImplementedException();

                    using (var transaction = context.Database.BeginTransaction()) {
                        foreach (var message in mailboxGroup) {
                            try {
                                var model = new MailMessage {
                                    Id = message.Id,
                                    MailboxId = mailboxGroup.Key.Id
                                };

                                context.MailMessages.Attach(model);
                                context.MailMessages.Remove(model);

                                // TODO: Extract column names from expression attributes.
                                // The content table is a virtual table and cannot be altered using EF.
                                await context.Database.ExecuteSqlCommandAsync(
                                    TransactionalBehavior.DoNotEnsureTransaction,
                                    "DELETE FROM mail_message_content WHERE message_id = @id;",
                                    new SQLiteParameter("@id", message.Id)).ConfigureAwait(false);

                                await context.SaveChangesAsync(OptimisticConcurrencyStrategy.ClientWins);

                                transaction.Commit();
                            } catch (Exception ex) {
                                Logger.ErrorException(ex.Message, ex);
                                transaction.Rollback();
                            }
                        }
                    }
                }
            } finally {
                Logger.Exit();
            }
        }

        private void OnMessagesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            try {
                RaisePropertyChanged(() => Messages);
                RaisePropertyChanged(() => MessageCount);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        public object Alphabet {
            get {
                return Enumerable.Range(65, 26)
                    .Select(Convert.ToChar).ToArray();
            }
        }

        internal async Task LoadContactsAsync() {
            Logger.Enter();

            Application.Current.AssertUIThread();

            try {
                var contacts = await Task.Run(() => {
                    using (var database = new DatabaseContext()) {
                        return database.MailContacts.ToArrayAsync();
                    }
                });

                var contexts = contacts.Select(x => new MailContactContext(x));
                _contacts.AddRange(contexts);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                Logger.Exit();
            }
        }

        /// <summary>
        ///     Queries the message source.
        /// </summary>
        /// <returns>Returns a task object.</returns>
        internal async Task QueryMessageSourceAsync() {
            Logger.Enter();

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
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                Logger.Exit();
            }
        }

        #endregion

        #region Public Events

        public event EventHandler MailboxSelectionChanged;

        private void OnMailboxSelectionChanged() {
            try {
                var handler = MailboxSelectionChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);

                if (SelectedMailbox == null) {
                    return;
                }

                _messages.Clear();

                Source = null;
                MessageSource = SelectedMailbox;
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
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
        }

        internal event EventHandler ContactSelectionChanged;

        internal void OnContactSelectionChanged() {
            var handler = ContactSelectionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);

            RaisePropertyChanged(() => SelectedContact);
            RaisePropertyChanged(() => SelectedContacts);
        }

        #endregion

        #region Properties

        [ImportMany]
        public IEnumerable<Theme> Themes { get; set; }

        public IEnumerable<NavigationContext> NavigationOptions {
            get { return _navigationOptions; }
        }

        public IEnumerable<MailContactContext> Contacts {
            get { return _contacts; }
        }

        public MailContactContext ActiveContact {
            get { return _activeContact; }
            set {
                if (_activeContact == value) {
                    return;
                }

                _activeContact = value;
                RaisePropertyChanged(() => ActiveContact);
            }
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

        public MailboxContext SelectedMailbox {
            get { return _selectedMailbox; }
            set {
                if (_selectedMailbox == value) {
                    return;
                }

                _selectedMailbox = value;
                RaisePropertyChanged(() => SelectedMailbox);
                OnMailboxSelectionChanged();
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

        internal void PublishAccount(MailAccountContext account) {
            Application.Current.AssertUIThread();

            _accounts.Add(account);
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
                OnMessageSourceChanged();
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

        public ICommand RestoreMessagesCommand {
            get { return _restoreMessagesCommand; }
        }

        public ICommand DeleteContactsCommand {
            get { return _deleteContactsCommand; }
        }

        public ICommand DeleteMessagesCommand {
            get { return _deleteMessagesCommand; }
        }

        public ICommand MarkMessagesAsSeenCommand {
            get { return _markMessagesAsSeenCommand; }
        }

        public ICommand MarkMessagesAsUnseenCommand {
            get { return _markMessagesAsUnseenCommand; }
        }

        public ICommand UnflagMessagesCommand {
            get { return _unflagMessagesCommand; }
        }

        public ICommand FlagMessagesCommand {
            get { return _flagMessagesCommand; }
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

        internal void QueryMessages(string text) {
            try {
                if (string.IsNullOrEmpty(text)) {
                    MessageSource = SelectedMailbox;
                } else {
                    MessageSource = new MessageQuery(text);
                }
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnAccountsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            try {
                RaisePropertyChanged(() => Accounts);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        internal Task RefreshMessageViewAsync() {
            return ViewMessageAsync(SelectedMessage);
        }

        internal async Task ViewMessageAsync(MailMessageContext message) {
            Logger.Enter();

            Application.Current.AssertUIThread();

            try {
                var hasMime = await message.GetIsMimeStoredAsync();
                if (!hasMime) {
                    await message.FetchAndDecryptAsync();
                }

                await message.DetailAsync();

                // We need to check after all awaits whether the selected message is still the same.
                if (SelectedMessage != null && SelectedMessage.Id == message.Id) {
                    Source = string.Format(message.IsExternalContentAllowed
                        ? "message:///{0}?blockExternals=false"
                        : "message:///{0}", message.Id);
                }
            } catch (MessageDecryptionFailedException ex) {
                // TODO: Notify user key is missing.
                Logger.ErrorException(ex.Message, ex);
            } catch (MissingKeyException ex) {
                // TODO: Notify user key is missing.
                Logger.ErrorException(ex.Message, ex);
            } catch (SignetMissingOrCorruptException ex) {
                // TODO: Notify user signet is missing or corrupt.
                Logger.ErrorException(ex.Message, ex);
            } catch (MissingContactException ex) {
                // TODO: Notify user the contact is not listed in the database.
                Logger.ErrorException(ex.Message, ex);
            } catch (Exception ex) {
                // TODO: General exception handling.
                Logger.ErrorException(ex.Message, ex);
            } finally {
                Logger.Exit();
            }
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
                var refresh = context.CountGlobalUnseenAsync();

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
                await Task.WhenAll(online.Concat(new[] { refresh }));

                _outboxTimer.Start();

                OnInitialized();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
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
                    x.LoadMailboxesAsync()
                });

                _accounts.AddRange(contexts);

                await Task.WhenAll(queries);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
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
                    await QueryMessageSourceAsync();
                }
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnCreateAccount(object obj) {
            try {
                var account = new MailAccountContext(new MailAccount());
                NavigationArguments.Push(account);

                var uri = typeof(CreateAccountStartFlyoutPage).ToPageUri();
                OnFlyoutNavigationRequested(new NavigationRequestedEventArgs(uri));
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
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
                await ProcessOutboxAsync();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private async Task ProcessOutboxAsync() {
            var getCompositions = Task.Run(async () => {
                using (var context = new DatabaseContext()) {
                    return await context.MailCompositions.Include(x => x.Addresses)
                                .Include(x => x.Attachments)
                                .ToArrayAsync();
                }
            });

            var getKeyPairs = Task.Run(async () => {
                using (var context = new DatabaseContext()) {
                    return await context.KeyPairs
                        .OrderByDescending(x => x.Date)
                        .ToArrayAsync();
                }
            });

            var keyPairs = await getKeyPairs;
            var compositions = await getCompositions;

            foreach (var composition in compositions.OrderBy(x => x.Created)) {
                var addresses = composition.Addresses.Select(x => x.Address).ToArray();
                var contacts = await Task.Run(async () => {
                    using (var context = new DatabaseContext()) {
                        return await context.MailContacts
                                .Include(x => x.Keys)
                                .Where(x => addresses.Contains(x.Address))
                                .ToDictionaryAsync(z => z.Address.ToLower());
                    }
                });

                //var message = new 

                // TODO: @Sebastian ...

                //var from = composition.Addresses.First(x => x.Role == AddressRole.From);
                //var message = new System.Net.Mail.MailMessage {
                //    From = from.
                //};
                //    var message = new MailMessage {
                //        IsBodyHtml = true,
                //        Subject = Subject,
                //        BodyEncoding = Encoding.UTF8,
                //        BodyTransferEncoding = TransferEncoding.Base64,
                //        From = new MailAddress(account.Address, account.Name),
                //        Body = await document
                //    };

                //    var signet = string.Format("pkey={0};", Convert.ToBase64String(current.PublicKey));
                //    message.Headers.Add(MessageHeaders.Signet, signet);

                //    message.To.Add(new MailAddress(address));
                //    foreach (var a in Attachments) {
                //        message.Attachments.Add(new Attachment(a.FullName));
                //    }

                //    // IO heavy operation, needs to run in background thread.
                //    var m = message;
                //    await Task.Run(() => m.PackageEmbeddedContent());

                //    if (key == null) {
                //        messages.Add(message);
                //    } else {
                //        var nonce = PublicKeyCrypto.GenerateNonce();
                //        var payload = await EncryptMessageAsync(message, current, key, nonce);
                //        message = GenerateDeliveryMessage(account, current.PublicKey, address, nonce);
                //        message.AlternateViews.Add(new AlternateView(new MemoryStream(payload),
                //            new ContentType(MediaTypes.EncryptedMime)));
                //        messages.Add(message);
                //    }
                //}
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
                Logger.ErrorException(ex.Message, ex);
            }
        }

        internal void NotifyOutboxNotEmpty() {

        }

        internal void NotifyContactsCreated(ICollection<MailContactContext> contacts) {
            Application.Current.AssertUIThread();

            _contacts.AddRange(contacts);
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
            Compose(new List<string>());
        }

        internal void Compose(IEnumerable<string> addresses) {
            var owner = Application.Current.MainWindow;
            var window = new CompositionWindow { ShowActivated = true };
            window.MimicOwnership(Application.Current.MainWindow);
            window.PrepareAsNew(addresses);

            if (owner.WindowState == WindowState.Maximized) {
                window.WindowState = WindowState.Maximized;
            }

            window.Show();
        }

        internal void InspectMessage(FileInfo file) {
            var owner = Application.Current.MainWindow;
            var inspector = new InspectionWindow(new FileMessageContext(file));
            inspector.MimicOwnership(owner);

            if (owner.WindowState == WindowState.Maximized) {
                inspector.WindowState = WindowState.Maximized;
            }

            inspector.Show();
        }

        internal void InspectMessage(MailMessageContext message) {
            var owner = Application.Current.MainWindow;
            var inspector = new InspectionWindow(message);
            inspector.MimicOwnership(owner);

            if (owner.WindowState == WindowState.Maximized) {
                inspector.WindowState = WindowState.Maximized;
            }

            inspector.Show();
        }

        internal void ShowMessage(MailMessageContext message) {
            if (!message.Mailbox.IsSelected) {
                message.Mailbox.IsSelected = true;
            }

            _messages.ForEach(x => x.IsSelected = false);
            message.IsSelected = true;
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

        internal void NotifyAccountsDeleted(IEnumerable<MailAccountContext> accounts) {
            Application.Current.AssertUIThread();

            foreach (var account in accounts) {
                _accounts.Remove(account);
            }
        }

        internal void NotifySeenStatesChanged(ICollection<Int64> seenIds, Int64 mailboxId) {
            foreach (var message in
                from message in _messages.Where(x => x.Mailbox.Id == mailboxId)
                select message) {
                message.IsSeen = seenIds.Contains(message.Id);
            }
        }

        public void NotifyAccountRemoved(MailAccountContext context) {
            _accounts.Remove(context);

            var messages = _messages.Where(x => x.Mailbox.Account.Id == context.Id).ToDictionary(x => x.Id);
            _messages.DeferNotifications = true;
            foreach (var message in messages) {
                _messages.Remove(message.Value);
            }
            _messages.DeferNotifications = false;
            _messages.NotifyCollectionChanged();

            if (Source == null) {
                return;
            }

            const string pattern = "message:///(?<ID>[0-9]+)";
            var match = Regex.Match(Source, pattern, RegexOptions.IgnoreCase);
            if (match.Success && messages.ContainsKey(Int64.Parse(match.Groups["ID"].Value))) {
                Source = null;
            }
        }

        internal void NotifyMessagesReceived(IEnumerable<MailMessageContext> messages) {
            // Don't mess with the current query.
            if (MessageSource is MessageQuery) {
                return;
            }

            var mailbox = SelectedMailbox;
            if (mailbox == null) {
                return;
            }

            var eligible = messages.Where(x => x.Mailbox.Name == mailbox.Name).ToArray();

            _messages.DeferNotifications = true;
            _messages.AddRange(eligible);
            _messages.DeferNotifications = false;
            _messages.NotifyCollectionChanged();
        }

        public void NotifyAccountCreated(MailAccountContext account) {
            _accounts.Add(account);
        }
    }
}
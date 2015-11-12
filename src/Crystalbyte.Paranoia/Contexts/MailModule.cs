using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using NLog;
using Crystalbyte.Paranoia.UI;
using System.Composition;
using System.Windows.Threading;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Crystalbyte.Paranoia.UI.Commands;
using System.Collections.Generic;
using System.Windows;
using System.Data.SQLite;
using Crystalbyte.Paranoia.Data.SQLite;
using System.Collections.Specialized;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Crystalbyte.Paranoia {

    [Export(typeof(Module)), Shared]
    public sealed class MailModule : Module {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private string _source;
        private float _zoomLevel;
        private bool _showOnlyUnseen;
        private bool _isSortAscending;
        private IMessageSource _messageSource;
        private MailboxContext _selectedMailbox;
        private readonly DispatcherTimer _outboxTimer;

        private readonly ICommand _createAccountCommand;
        private readonly ICommand _flagMessagesCommand;
        private readonly ICommand _unflagMessagesCommand;
        private readonly ICommand _deleteMessagesCommand;
        private readonly ICommand _restoreMessagesCommand;
        private readonly ICommand _markMessagesAsSeenCommand;
        private readonly ICommand _markMessagesAsUnseenCommand;
        private readonly ICommand _deleteContactsCommand;

        private readonly DeferredObservableCollection<MailContactContext> _contacts;
        private readonly DeferredObservableCollection<MailMessageContext> _messages;
        private readonly ObservableCollection<MailAccountContext> _accounts;

        #endregion

        #region Construction

        public MailModule() {
            _accounts = new DeferredObservableCollection<MailAccountContext>();
            _accounts.CollectionChanged += OnAccountsCollectionChanged;

            _messages = new DeferredObservableCollection<MailMessageContext>();
            _messages.CollectionChanged += OnMessagesCollectionChanged;

            _contacts = new DeferredObservableCollection<MailContactContext>();
            _contacts.CollectionChanged += (sender, e) => RaisePropertyChanged(() => Contacts);

            _deleteContactsCommand = new DeleteContactsCommand(this);
            _restoreMessagesCommand = new RestoreMessagesCommand(this);
            _markMessagesAsSeenCommand = new FlagMessagesAsSeenCommand(this);
            _deleteMessagesCommand = new DeleteMessagesCommand(this);
            _markMessagesAsUnseenCommand = new FlagMessagesAsUnseenCommand(this);
            _createAccountCommand = new RelayCommand(OnCreateAccount);
            _flagMessagesCommand = new FlagMessagesCommand(this);
            _unflagMessagesCommand = new UnflagMessagesCommand(this);

            _outboxTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _outboxTimer.Tick += OnOutboxTimerTick;

            _showOnlyUnseen = false;
        }

        #endregion

        #region Imports

        [Import]
        public IViewManager ViewManager { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {

        }

        #endregion

        #region Class Overrides

        internal override async Task InitializeAsync() {
            ViewManager.RegisterView(new MailView(this) { IsSelected = true });
            ViewManager.RegisterView(new ContactsView(this));

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
            await Task.WhenAll(online);

            _outboxTimer.Start();
        }

        internal void ResetZoom() {
            ZoomLevel = 0.0f;
        }

        #endregion

        #region Events

        internal event EventHandler ContactSelectionChanged;

        internal void OnContactSelectionChanged() {
            var handler = ContactSelectionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);

            RaisePropertyChanged(() => SelectedContact);
            RaisePropertyChanged(() => SelectedContacts);
        }

        internal event EventHandler<ItemSelectionRequestedEventArgs> ItemSelectionRequested;

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

        internal event EventHandler MessageSelectionChanged;

        internal void OnMessageSelectionChanged() {
            var handler = MessageSelectionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);

            RaisePropertyChanged(() => SelectedMessage);
            RaisePropertyChanged(() => SelectedMessages);
            RaisePropertyChanged(() => IsMessageSelected);
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

                _messages.Clear();

                Source = null;
                MessageSource = SelectedMailbox;
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        #endregion

        #region Properties

        public ICommand DeleteContactsCommand {
            get { return _deleteContactsCommand; }
        }

        public IEnumerable<MailContactContext> Contacts {
            get { return _contacts; }
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

        public object Alphabet {
            get {
                return Enumerable.Range(65, 26)
                    .Select(Convert.ToChar).ToArray();
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

        public bool IsMessageSelected {
            get { return SelectedMessage != null; }
        }

        public MailMessageContext SelectedMessage {
            get {
                return SelectedMessages == null
                    ? null
                    : SelectedMessages.FirstOrDefault();
            }
        }

        public ICommand CreateAccountCommand {
            get { return _createAccountCommand; }
        }

        public ICommand RestoreMessagesCommand {
            get { return _restoreMessagesCommand; }
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

        public IEnumerable<MailMessageContext> SelectedMessages {
            get { return _messages.Where(x => x.IsSelected).ToArray(); }
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

        public IEnumerable<MailMessageContext> Messages {
            get { return _messages; }
        }

        public int MessageCount {
            get { return _messages.Count; }
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

        public float ZoomLevel {
            get { return _zoomLevel; }
            set {
                if (Math.Abs(_zoomLevel - value) <= float.Epsilon) {
                    return;
                }

                _zoomLevel = value;
                RaisePropertyChanged(() => ZoomLevel);
            }
        }

        #endregion

        #region Methods

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
            App.Context.NavigateModalToPage(uri);
        }

        private void OnMessagesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            try {
                RaisePropertyChanged(() => Messages);
                RaisePropertyChanged(() => MessageCount);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
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

        private static async Task DeleteFlagsFromStoreAsync(IEnumerable<long> ids, string value) {
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

        private static async Task SaveFlagsToStoreAsync(IEnumerable<long> ids, string value) {
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

                await message.LoadDetailsAsync();

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

        internal void NorifyAccountAdded(MailAccountContext account) {
            Application.Current.AssertUIThread();

            _accounts.Add(account);
        }

        public IEnumerable<MailAccountContext> Accounts {
            get { return _accounts; }
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

        private static async Task DeleteStoredMessagesAsync(IGrouping<MailboxContext, MailMessageContext> mailboxGroup) {
            Logger.Enter();

            Application.Current.AssertBackgroundThread();

            try {
                using (var context = new DatabaseContext()) {
                    await context.OpenAsync();
                    await context.EnableForeignKeysAsync();

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

        private void NotifyMessagesRemoved(IEnumerable<MailMessageContext> messages) {
            Application.Current.AssertUIThread();

            _messages.DeferNotifications = true;
            foreach (var message in messages) {
                _messages.Remove(message);
            }

            _messages.DeferNotifications = false;
            _messages.NotifyCollectionChanged();
        }

        public async Task<int> CountGlobalUnseenAsync() {
            var start = Environment.TickCount & int.MaxValue;

            var hits = await Task.Run(() => {
                using (var context = new DatabaseContext()) {
                    return context.MailMessages
                        .Where(x => x.Flags.All(y => y.Value != MailMessageFlags.Seen))
                        .CountAsync();
                }
            });

            var finish = Environment.TickCount & Int32.MaxValue;
            if (finish - start > 1000) {
                Logger.Warn(Resources.QueryPerformanceTemplate, finish - start / 1000.0f);
            }

            return hits;
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

        private async Task SendCompositionAsync(MailComposition composition) {
            var account = await Task.Run(() => {
                using (var context = new DatabaseContext()) {
                    var accounts = context.Set<MailAccount>();
                    return accounts.FirstOrDefaultAsync(x => x.Id == composition.AccountId);
                }
            });

            if (account == null) {
                throw new MissingAccountException();
            }

            await Task.Run(async () => {
                var message = composition.ToMessage(account.Address);
                var bytes = await message.ToMimeAsync();

                var messages = new List<System.Net.Mail.MailMessage>();
                foreach (var address in composition.Addresses) {
                    using (var context = new DatabaseContext()) {
                        var contacts = context.Set<MailContact>();
                        var contact = await contacts.FirstOrDefaultAsync(x => x.Address == address.Address);

                        var keys = await context.Set<KeyPair>().ToArrayAsync();

                        // The contact is not yet saved in our database, thus there are no keys to start encryption.
                        // We need to send it in plain text and remove all other recipients.
                        if (contact == null || contact.Keys.Count == 0) {
                            messages.Add(message);
                            message.AttachPublicKeys(keys);
                        } else {
                            var cypher = new SodiumHybridMimeCypher();
                            var data = cypher.Encrypt(contact, bytes);
                            var wrapper = await message.WrapSodiumEncryptedMessageAsync(data);
                            messages.Add(wrapper);
                            wrapper.AttachPublicKeys(keys);
                        }
                    }
                }


                using (var connection = new SmtpConnection()) {
                    using (var auth = await connection.ConnectAsync(account.SmtpHost, account.SmtpPort)) {
                        using (var session = await auth.LoginAsync(account.SmtpUsername, account.SmtpPassword)) {
                            foreach (var m in messages) {
                                await session.SendAsync(m);

                                using (var context = new DatabaseContext()) {
                                    var compositions = context.Set<MailComposition>();
                                    compositions.Attach(composition);
                                    compositions.Remove(composition);

                                    var contacts = context.Set<MailContact>();

                                    Debug.Assert(m.To.Count == 1);
                                    var address = m.To.First().Address;
                                    var contact = await contacts.FirstOrDefaultAsync(x => x.Address == address);
                                    contact.Relevance++;

                                    await context.SaveChangesAsync(OptimisticConcurrencyStrategy.ClientWins);
                                }
                            }
                        }
                    }
                }
            });
        }

        internal async Task ProcessOutboxAsync() {
            try {
                using (var context = new DatabaseContext()) {
                    var compositions = await context.Set<MailComposition>()
                        .Include(x => x.Addresses)
                        .Include(x => x.Attachments)
                        .ToArrayAsync();

                    foreach (var composition in compositions) {
                        await SendCompositionAsync(composition);
                    }
                }
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
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
            await window.PrepareAsForwardAsync(new Dictionary<string, string> {
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
            await window.PrepareAsForwardAsync(new Dictionary<string, string> {
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
            await window.PrepareAsReplyAllAsync(new Dictionary<string, string> {
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
            await window.PrepareAsReplyAllAsync(new Dictionary<string, string> {
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

        internal void DisplayMessage(MailMessageContext message) {
            if (!message.Mailbox.IsSelected) {
                message.Mailbox.IsSelected = true;
            }

            _messages.ForEach(x => x.IsSelected = false);
            message.IsSelected = true;
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
            if (match.Success && messages.ContainsKey(long.Parse(match.Groups["ID"].Value))) {
                Source = null;
            }
        }

        internal void NotifyContactsCreated(ICollection<MailContactContext> contacts) {
            Application.Current.AssertUIThread();

            _contacts.AddRange(contacts);
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

            var eligible = messages
                .Where(x => x.Mailbox.Name == mailbox.Name)
                .ToArray();

            _messages.DeferNotifications = true;
            _messages.AddRange(eligible);
            _messages.DeferNotifications = false;
            _messages.NotifyCollectionChanged();
        }

        public void NotifyAccountCreated(MailAccountContext account) {
            _accounts.Add(account);
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
                App.Context.NavigateFlyoutToPage(uri);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
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

                var contexts = contacts.Select(x => new MailContactContext(x))
                    .ToArray();
                _contacts.DeferNotifications = true;
                _contacts.AddRange(contexts);
                _contacts.DeferNotifications = false;
                _contacts.NotifyCollectionChanged();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                Logger.Exit();
            }
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

        internal void UnloadContacts() {
            _contacts.DeferNotifications = true;
            _contacts.Clear();
            _contacts.DeferNotifications = false;
            _contacts.NotifyCollectionChanged();
        }

        #endregion
    }
}

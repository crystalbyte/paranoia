#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Composition;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Crystalbyte.Paranoia.Cryptography;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Net;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI;
using Crystalbyte.Paranoia.UI.Commands;
using Crystalbyte.Paranoia.UI.Pages;
using Newtonsoft.Json;
using NLog;

#endregion

namespace Crystalbyte.Paranoia {
    [Export, Shared]
    public sealed class AppContext : NotificationObject {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private float _zoom;
        private bool _showAllMessages;
        private string _queryString;
        private string _source;
        private string _statusText;
        private bool _isPopupVisible;
        private MailAccountContext _transitAccount;
        private readonly DispatcherTimer _outboxTimer;
        private MailboxContext _selectedMailbox;
        private OutboxContext _selectedOutbox;
        private readonly ObservableCollection<AttachmentContext> _attachments;
        private readonly DeferredObservableCollection<MailMessageContext> _messages;
        private readonly ObservableCollection<MailAccountContext> _accounts;
        private readonly ObservableCollection<MailContactContext> _contacts;
        private readonly ObservableCollection<NavigationContext> _navigationOptions;
        private readonly ICommand _resetZoomCommand;
        private readonly ICommand _restoreMessagesCommand;
        private readonly ICommand _markAsSeenCommand;
        private readonly ICommand _markAsNotSeenCommand;
        private readonly ICommand _markAsFlaggedCommand;
        private readonly ICommand _markAsNotFlaggedCommand;
        private readonly ICommand _blockContactsCommand;
        private readonly ICommand _unblockContactsCommand;
        private readonly ICommand _createAccountCommand;
        private readonly ICommand _createContactCommand;
        private readonly ICommand _deleteContactsCommand;
        private readonly ICommand _deleteMessagesCommand;
        private readonly ICommand _refreshKeysCommand;
        private bool _isSortAscending;
        private string _queryContactString;
        private bool _isAnimating;


        #endregion

        #region Construction

        public AppContext() {
            _accounts = new ObservableCollection<MailAccountContext>();
            _accounts.CollectionChanged += OnAccountsCollectionChanged;

            _messages = new DeferredObservableCollection<MailMessageContext>();
            _messages.CollectionChanged += OnMessagesCollectionChanged;

            _attachments = new ObservableCollection<AttachmentContext>();

            _navigationOptions = new ObservableCollection<NavigationContext>
            {
                new NavigationContext { Title = Resources.MessagesTitle, TargetUri = typeof(MessagesPage).ToPageUri(), IsSelected = true},
                new NavigationContext { Title = Resources.ContactsTitle, TargetUri = typeof(ContactsPage).ToPageUri() }
            };

            _restoreMessagesCommand = new RestoreMessagesCommand(this);
            _markAsSeenCommand = new MarkAsSeenCommand(this);
            _refreshKeysCommand = new RelayCommand(OnRefreshKeys);
            _blockContactsCommand = new BlockContactsCommand(this);
            _deleteMessagesCommand = new DeleteMessagesCommand(this);
            _markAsNotSeenCommand = new MarkAsNotSeenCommand(this);
            _deleteContactsCommand = new DeleteContactsCommand(this);
            _createAccountCommand = new RelayCommand(OnCreateAccount);
            _createContactCommand = new RelayCommand(OnCreateContact);
            _resetZoomCommand = new RelayCommand(p => Zoom = 100.0f);
            _unblockContactsCommand = new UnblockContactsCommand(this);
            _markAsFlaggedCommand = new MarkAsFlaggedCommand(this);
            _markAsNotFlaggedCommand = new MarkAsNotFlaggedCommand(this);

            NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;

            Observable.Timer(TimeSpan.FromHours(2))
                .Subscribe(OnRefreshKeys);

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

            _zoom = 1.0f;
            _showAllMessages = true;
        }

        private async void OnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e) {
            if (!e.IsAvailable)
                return;

            foreach (var account in Accounts) {
                await account.TakeOnlineAsync();
            }
        }

        private async void OnContactQueryReceived(string query) {
            await FilterContactsAsync(query);
        }

        private void OnMessagesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            RaisePropertyChanged(() => Messages);
            RaisePropertyChanged(() => MessageCount);
            RaisePropertyChanged(() => IsMessageOrSmtpRequestSelected);
        }

        private async void OnRefreshKeys(object obj) {
            await RefreshKeysForAllContactsAsync();
        }

        private async void OnRefreshKeys(long obj) {
            await RefreshKeysForAllContactsAsync();
        }

        private async Task RefreshKeysForAllContactsAsync() {
            try {
                StatusText = Resources.RefreshingPublicKeysStatus;

                IEnumerable<MailContactModel> contacts;
                using (var database = new DatabaseContext()) {
                    contacts = await database.MailContacts.ToArrayAsync();
                }

                using (var client = new WebClient()) {
                    foreach (var contact in contacts) {
                        var entry = await DownloadKeysForContactAsync(contact, client);
                        if (entry == null || entry.Keys == null) {
                            continue;
                        }
                        await UpdateKeysInDatabaseForContactAsync(contact, entry);
                    }
                }

                foreach (var contact in _contacts) {
                    await contact.NotifyKeysUpdatedAsync();
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            } finally {
                StatusText = Resources.ReadyStatus;
            }
        }

        private static async Task UpdateKeysInDatabaseForContactAsync(MailContactModel contact, KeyCollection collection) {
            try {
                using (var database = new DatabaseContext()) {
                    var keys = await database.PublicKeys.Where(x => x.ContactId == contact.Id).ToArrayAsync();
                    var keysToBeAdded = collection.Keys.Except(keys.Select(x => x.Data));
                    foreach (var key in keysToBeAdded) {
                        database.PublicKeys.Add(new PublicKeyModel {
                            ContactId = contact.Id,
                            Data = key
                        });
                    }

                    await database.SaveChangesAsync();
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private static async Task<KeyCollection> DownloadKeysForContactAsync(MailContactModel contact, WebClient client) {
            var server = Settings.Default.KeyServer;
            var address = String.Format("{0}/keys?email={1}", server, contact.Address);

            var uri = new Uri(address, UriKind.Absolute);
            client.Headers.Add(HttpRequestHeader.UserAgent, Settings.Default.UserAgent);
            var response = await client.OpenReadTaskAsync(uri);

            using (var reader = new StreamReader(response)) {
                var json = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<KeyCollection>(json);
            }
        }

        public object Alphabet {
            get {
                return Enumerable.Range(65, 26)
                    .Select(Convert.ToChar).ToArray();
            }
        }


        internal async Task FilterContactsAsync(string query) {
            _contacts.Clear();

            if (String.IsNullOrWhiteSpace(query)) {
                await LoadContactsAsync();
                return;
            }

            IEnumerable<MailContactModel> contacts;
            using (var database = new DatabaseContext()) {
                contacts = await database.MailContacts
                    .Where(x => x.Name.Contains(query)
                        || x.Address.Contains(query))
                    .ToArrayAsync();
            }

            _contacts.AddRange(contacts.Select(x => new MailContactContext(x)));
        }

        internal async Task LoadContactsAsync() {
            IEnumerable<MailContactModel> contacts;
            using (var database = new DatabaseContext()) {
                contacts = await database.MailContacts.ToArrayAsync();
            }

            _contacts.AddRange(contacts.Select(x => new MailContactContext(x)));
            foreach (var contact in _contacts) {
                await contact.CheckSecurityStateAsync();
            }
        }

        /// <summary>
        ///     Queries the message source.
        /// </summary>
        /// <param name="source">The message source to query.</param>
        /// <returns>Returns a task object.</returns>
        private async Task RequestMessagesAsync(IMessageSource source) {
            Application.Current.AssertBackgroundThread();

            await Application.Current.Dispatcher.InvokeAsync(() => _messages.Clear());
            var messages = await source.GetMessagesAsync();
            await Application.Current.Dispatcher.InvokeAsync(() => {
                _messages.DeferNotifications = true;
                _messages.AddRange(messages);
                _messages.DeferNotifications = false;
                _messages.NotifyCollectionChanged();
            });

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

                ClearMessagesAndSmtpRequests();
                await RefreshViewForSelectedOutbox();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void ClearMessagesAndSmtpRequests() {
            _messages.Clear();
            _accounts.ForEach(x => x.Outbox.ClearSmtpRequests());
            Source = "about:blank";
        }

        public event EventHandler MailboxSelectionChanged;

        private async void OnMailboxSelectionChanged() {
            try {
                var handler = MailboxSelectionChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);

                if (SelectedMailbox == null) {
                    return;
                }

                ClearMessagesAndSmtpRequests();
                await RefreshViewForSelectedMailboxAsync();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private async Task RefreshViewForSelectedOutbox() {
            await RequestOutboxContentAsync(SelectedOutbox);
        }

        private async Task RefreshViewForSelectedMailboxAsync() {
            await Task.Run(() => RequestMessagesAsync(SelectedMailbox));
            await Task.Run(() => SelectedMailbox.SyncMessagesAsync());
        }

        internal event EventHandler FlyOutClosing;

        private void OnFlyOutClosing() {
            var handler = FlyOutClosing;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        internal event EventHandler FlyOutClosed;

        internal void OnFlyOutClosed() {
            var handler = FlyOutClosed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public event EventHandler SortOrderChanged;

        private void OnSortOrderChanged() {
            var handler = SortOrderChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        internal event EventHandler<NavigationRequestedEventArgs> PopupNavigationRequested;

        private void OnPopupNavigationRequested(NavigationRequestedEventArgs e) {
            var handler = PopupNavigationRequested;
            if (handler != null)
                handler(this, e);
        }

        internal event EventHandler<NavigationRequestedEventArgs> FlyOutNavigationRequested;

        private void OnFlyOutNavigationRequested(NavigationRequestedEventArgs e) {
            var handler = FlyOutNavigationRequested;
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

        private async Task RefreshViewChangedQueryString(string query) {
            if (String.IsNullOrWhiteSpace(query)) {
                await Task.Run(() => RequestMessagesAsync(SelectedMailbox));
            } else {
                await Task.Run(() => RequestMessagesAsync(new QueryContext(query)));
            }
        }

        private static async Task RequestOutboxContentAsync(OutboxContext outbox) {
            await outbox.LoadSmtpRequestsAsync();
        }

        #endregion

        #region Property Declarations

        public IEnumerable<NavigationContext> NavigationOptions {
            get { return _navigationOptions; }
        }

        public IEnumerable<MailContactContext> Contacts {
            get { return _contacts; }
        }

        public ICollection<AttachmentContext> Attachments {
            get { return _attachments; }
        }

        internal void ConfigureAccount(Uri uri) {
            OnFlyOutNavigationRequested(new NavigationRequestedEventArgs(uri));
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

        public MailAccountContext TransitAccount {
            get { return _transitAccount; }
            set {
                if (_transitAccount == value) {
                    return;
                }

                _transitAccount = value;
                RaisePropertyChanged(() => TransitAccount);
            }
        }

        internal void NotifyAccountCreated(MailAccountContext account) {
            _accounts.Add(account);
        }

        public IEnumerable<MailAccountContext> Accounts {
            get { return _accounts; }
        }

        public bool IsMessageSelected {
            get { return SelectedMessage != null; }
        }

        public string StatusText {
            get { return _statusText; }
            set {
                if (_statusText == value) {
                    return;
                }

                _statusText = value;
                RaisePropertyChanged(() => StatusText);
            }
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

        public bool ShowAllMessages {
            get { return _showAllMessages; }
            set {
                if (_showAllMessages == value) {
                    return;
                }
                _showAllMessages = value;
                RaisePropertyChanged(() => ShowAllMessages);
                OnMessageFilterChanged();
            }
        }

        private async void OnMessageFilterChanged() {
            var mailbox = SelectedMailbox;
            if (mailbox == null) {
                return;
            }

            mailbox.ShowAllMessages = ShowAllMessages;
            await Task.Run(() => RequestMessagesAsync(mailbox));
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

        public ICommand RefreshKeysCommand {
            get { return _refreshKeysCommand; }
        }

        public ICommand CreateContactCommand {
            get { return _createContactCommand; }
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

        public PublicKeyCrypto KeyContainer {
            get;
            private set;
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

        private async void OnQueryReceived(string text) {
            try {
                await RefreshViewChangedQueryString(text);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnAccountsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            RaisePropertyChanged(() => Accounts);
        }

        private async void OnMessageSelectionReceived(EventPattern<object> obj) {
            Application.Current.AssertUIThread();

            ClearPreviewArea();
            var message = SelectedMessages.FirstOrDefault();
            if (message == null) {
                return;
            }

            try {
                await RefreshMessageSelectionAsync(message);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private async Task RefreshMessageSelectionAsync(MailMessageContext message) {
            await MarkSelectionAsSeenAsync();
            if (!await message.GetIsMimeLoadedAsync()) {
                await message.DownloadMessageAsync();
            }
            await ViewMessageAsync(message);
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
            var uri = typeof(BlankPage).ToPageUri();
            OnPopupNavigationRequested(new NavigationRequestedEventArgs(uri));
            IsPopupVisible = false;
        }

        private void ClearPreviewArea() {
            Application.Current.AssertUIThread();

            _attachments.Clear();
            Source = null;
        }

        private async Task DisplayAttachmentAsync(MailMessageContext message) {
            var attachmentContexts = new List<AttachmentContext>();
            using (var context = new DatabaseContext()) {
                var mimeMessage = await context.MimeMessages.FirstOrDefaultAsync(x => x.MessageId == message.Id);
                if (mimeMessage == null)
                    return;

                var reader = new MailMessageReader(mimeMessage.Data);
                var attachments = reader.FindAllAttachments();

                attachments.Where(x => x.ContentId == null)
                    .ForEach(y => attachmentContexts.Add(new AttachmentContext(y)));
            }
            Application.Current.Dispatcher.Invoke(() => {
                Attachments.Clear();
                Attachments.AddRange(attachmentContexts);
            });
        }

        private async Task ViewMessageAsync(MailMessageContext message) {
            Source = string.Format("asset://paranoia/message/{0}", message.Id);
            await DisplayAttachmentAsync(message);
        }

        internal async Task ViewUnblockedMessageAsync(MailMessageContext message) {
            Source = string.Format("asset://paranoia/message/{0}?blockExternals=false", message.Id);
            await DisplayAttachmentAsync(message);
        }

        internal static DirectoryInfo GetKeyDirectory() {
            var dataDir = (string)AppDomain.CurrentDomain.GetData("DataDirectory");
            return new DirectoryInfo(Path.Combine(dataDir, "keys"));
        }

        public async Task RunAsync() {
            await LoadContactsAsync();
            await LoadAccountsAsync();

            await RefreshKeysForAllContactsAsync();
            foreach (var account in Accounts) {
                await account.TakeOnlineAsync();
            }

            _outboxTimer.Start();
        }

        private async Task LoadAccountsAsync() {
            using (var context = new DatabaseContext()) {
                var accounts = await context.MailAccounts.ToArrayAsync();
                _accounts.AddRange(accounts.Select(x => new MailAccountContext(x)));
            }

            foreach (var account in Accounts) {
                await account.LoadMailboxesAsync();
                await account.Outbox.LoadSmtpRequestsAsync();
            }
        }

        internal void CreateMailbox(IMailboxCreator parent) {
            NavigationStore.Push(typeof(CreateMailboxPage), parent);

            var uri = typeof(CreateMailboxPage).ToPageUri();
            OnPopupNavigationRequested(new NavigationRequestedEventArgs(uri));
            IsPopupVisible = true;
        }

        internal void OnCreateContact(object obj) {
            var uri = typeof(CreateContactPage).ToPageUri();
            OnPopupNavigationRequested(new NavigationRequestedEventArgs(uri));
            IsPopupVisible = true;
        }

        internal void OnCreateKeyPair() {
            var uri = typeof(CreateKeyPage).ToPageUri();
            OnPopupNavigationRequested(new NavigationRequestedEventArgs(uri));
            IsPopupVisible = true;
        }

        private void OnCreateAccount(object obj) {
            var uri = typeof(CreateAccountPage).ToPageUri();
            OnFlyOutNavigationRequested(new NavigationRequestedEventArgs(uri));
        }

        internal async Task InitKeysAsync() {
            var info = GetKeyDirectory();
            var publicKey = Path.Combine(info.FullName, Settings.Default.PublicKeyFile);
            var privateKey = Path.Combine(info.FullName, Settings.Default.PrivateKeyFile);

            KeyContainer = new PublicKeyCrypto();
            await KeyContainer.InitFromFileAsync(publicKey, privateKey);
        }

        internal void CloseFlyOut() {
            var uri = typeof(BlankPage).ToPageUri();
            OnFlyOutClosing();
            OnFlyOutNavigationRequested(new NavigationRequestedEventArgs(uri));
            OnFlyOutClosed();
        }

        internal void ClearViews() {
            _messages.Clear();
            ClearPreviewArea();
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

        internal async Task DeleteSelectedMessagesAsync() {
            try {
                var messages = SelectedMessages.ToArray();
                var accountGroups = messages.GroupBy(x => x.Mailbox.Account).ToArray();
                foreach (var accountGroup in accountGroups) {
                    var trash = accountGroup.Key.GetTrashMailbox();
                    if (trash == null) {
                        throw new InvalidOperationException("trash must not be null.");
                    }

                    var mailboxGroups = accountGroup.GroupBy(x => x.Mailbox).ToArray();
                    foreach (var mailboxGroup in mailboxGroups) {
                        var groupedMessages = mailboxGroup.ToArray();
                        await mailboxGroup.Key.DeleteMessagesAsync(groupedMessages, trash.Name);
                    }
                }
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
            var tasks = Accounts.Select(x => x.Outbox.ProcessOutgoingMessagesAsync());
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
                    || (SelectedOutbox != null && SelectedOutbox.SelectedSmtpRequest != null);
            }
        }

        internal void NotifySmtpRequestChanged() {
            RaisePropertyChanged(() => IsMessageOrSmtpRequestSelected);
        }

        public void ResetStatusText() {
            StatusText = Resources.ReadyStatus;
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

        internal void Reply() {
            Reply(SelectedMessage);
        }

        internal void Reply(MailMessageContext message) {
            var url = string.Format("?action=reply&id={0}", message.Id);
            var uri = typeof(ComposeMessagePage).ToPageUri(url);
            App.Context.Compose(uri);
        }

        internal void Reply(FileSystemInfo file) {
            var path = Uri.EscapeDataString(file.FullName);
            var url = string.Format("?action=reply&path={0}", path);
            var uri = typeof(ComposeMessagePage).ToPageUri(url);
            App.Context.Compose(uri);
        }

        internal void Forward() {
            Forward(SelectedMessage);
        }

        internal void Forward(MailMessageContext message) {
            var url = string.Format("?action=forward&id={0}", message.Id);
            var uri = typeof(ComposeMessagePage).ToPageUri(url);
            App.Context.Compose(uri);
        }

        internal void Forward(FileSystemInfo file) {
            var path = Uri.EscapeDataString(file.FullName);
            var url = string.Format("?action=forward&path={0}", path);
            var uri = typeof(ComposeMessagePage).ToPageUri(url);
            App.Context.Compose(uri);
        }

        internal void ReplyToAll() {
            ReplyToAll(SelectedMessage);
        }

        internal void ReplyToAll(MailMessageContext message) {
            var url = string.Format("?action=reply-all&id={0}", message.Id);
            var uri = typeof(ComposeMessagePage).ToPageUri(url);
            App.Context.Compose(uri);
        }

        internal void ReplyToAll(FileSystemInfo file) {
            var path = Uri.EscapeDataString(file.FullName);
            var url = string.Format("?action=reply-all&path={0}", path);
            var uri = typeof(ComposeMessagePage).ToPageUri(url);
            App.Context.Compose(uri);
        }

        internal void Compose(Uri uri) {
            var message = App.Context.SelectedMessage;
            if (message == null) {
                throw new InvalidOperationException();
            }

            var owner = Application.Current.MainWindow;
            var window = new CompositionWindow { Source = uri };
            window.MimicOwnership(owner);

            if (owner.WindowState == WindowState.Maximized) {
                window.WindowState = WindowState.Maximized;
            }

            window.Show();
        }

        internal void Print(string html) {
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

        internal void Compose() {
            var owner = Application.Current.MainWindow;
            var window = new CompositionWindow();
            window.MimicOwnership(Application.Current.MainWindow);
            window.Source = typeof(ComposeMessagePage).ToPageUri("?action=new");

            if (owner.WindowState == WindowState.Maximized) {
                window.WindowState = WindowState.Maximized;
            }

            window.Show();
        }

        internal async Task InspectMessageAsync(FileInfo file) {
            var owner = Application.Current.MainWindow;
            var inspector = new InspectionWindow {
                WindowState =
                    owner.WindowState == WindowState.Maximized
                    ? WindowState.Maximized
                    : WindowState.Normal
            };
            inspector.MimicOwnership(owner);
            await inspector.InitWithFileAsync(file);
            inspector.Show();
        }
        internal async Task InspectMessageAsync(MailMessageContext message) {
            var owner = Application.Current.MainWindow;
            var inspector = new InspectionWindow {
                WindowState =
                    owner.WindowState == WindowState.Maximized
                    ? WindowState.Maximized
                    : WindowState.Normal
            };
            inspector.MimicOwnership(owner);
            await inspector.InitWithMessageAsync(message);
            inspector.Show();
        }

        internal void ShowMessage(MailMessageContext message) {
            if (!message.Mailbox.IsSelected) {
                message.Mailbox.IsSelected = true;
            }

            _messages.ForEach(x => x.IsSelected = false);
            message.IsSelected = true;
        }

        internal void NotifyMessagesRemoved(IEnumerable<MailMessageContext> messages) {
            messages.ForEach(x => _messages.Remove(x));
        }

        internal void NotifyMessageRemoved(MailMessageContext message) {
            _messages.Remove(message);
        }

        internal async Task DeleteSelectedContactsAsync() {
            using (var database = new DatabaseContext()) {
                var contacts = _contacts.Select(x => new MailContactModel {
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

        internal async Task BlockSelectedUsersAsync() {
            await SetBlockForSelectedUsersAsync(true);
        }

        internal async Task UnblockSelectedUsersAsync() {
            await SetBlockForSelectedUsersAsync(false);
        }

        internal async Task SetBlockForSelectedUsersAsync(bool block) {
            var contacts = SelectedContacts.ToArray();

            using (var database = new DatabaseContext()) {
                foreach (var contact in contacts) {
                    try {
                        var c = await database.MailContacts.FindAsync(contact.Id);
                        c.IsBlocked = block;
                        contact.IsBlocked = block;
                    } catch (Exception ex) {
                        Logger.Error(ex);
                    }
                }

                await database.SaveChangesAsync();
            }
        }

        internal void NotifyAccountDeleted(MailAccountContext account) {
            _accounts.Remove(account);
            RaisePropertyChanged(() => Accounts);
        }

        internal Task MarkSelectionAsFlaggedAsync() {
            var messages = SelectedMessages.ToArray();

            return Task.Run(() => {
                var tasks = messages
                    .Where(x => x.IsSeen)
                    .GroupBy(x => x.Mailbox)
                    .Select(x => x.Key.MarkAsFlaggedAsync(x.ToArray()));

                Task.WhenAll(tasks);
            });
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
    }
}
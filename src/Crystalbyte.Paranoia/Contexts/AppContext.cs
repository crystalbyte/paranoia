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
using System.Windows.Input;
using System.Windows.Threading;
using Crystalbyte.Paranoia.Cryptography;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Net;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI;
using Crystalbyte.Paranoia.UI.Commands;
using Crystalbyte.Paranoia.UI.Pages;
using Newtonsoft.Json;
using NLog;
using Crystalbyte.Paranoia.Mail;
using System.Text;

#endregion

namespace Crystalbyte.Paranoia {
    [Export, Shared]
    public sealed class AppContext : NotificationObject {

        #region Private Fields

        private float _zoom;
        private bool _showAllMessages;
        private string _queryString;
        private string _source;
        private string _statusText;
        private bool _isPopupVisible;
        private bool _isAccountSelectionRequested;
        private MailAccountContext _transitAccount;
        private readonly DispatcherTimer _outboxTimer;
        private MailboxContext _selectedMailbox;
        private readonly ObservableCollection<AttachmentContext> _attachments;
        private readonly DeferredObservableCollection<MailMessageContext> _messages;
        private readonly ObservableCollection<MailAccountContext> _accounts;
        private readonly ObservableCollection<MailContactContext> _contacts;
        private MailContactContext _selectedContact;
        private readonly ICommand _printCommand;
        private readonly ICommand _resetZoomCommand;
        private readonly ICommand _replyCommand;
        private readonly ICommand _deleteCommand;
        private readonly ICommand _composeCommand;
        private readonly ICommand _forwardCommand;
        private readonly ICommand _restoreCommand;
        private readonly ICommand _markAsSeenCommand;
        private readonly ICommand _markAsNotSeenCommand;
        private readonly ICommand _configAccountCommand;
        private readonly ICommand _createAccountCommand;
        private readonly ICommand _selectAccountCommand;
        private readonly ICommand _createContactCommand;
        private readonly ICommand _deleteContactCommand;
        private readonly ICommand _refreshKeysCommand;
        private bool _isAllContactsSelected;
        private bool _isSortAscending;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        #endregion

        #region Construction

        public AppContext() {
            _accounts = new ObservableCollection<MailAccountContext>();
            _accounts.CollectionChanged += OnAccountsCollectionChanged;

            _messages = new DeferredObservableCollection<MailMessageContext>();
            _messages.CollectionChanged += OnMessagesCollectionChanged;

            _attachments = new ObservableCollection<AttachmentContext>();

            _printCommand = new PrintCommand(this);
            _replyCommand = new ReplyCommand(this);
            _forwardCommand = new ForwardCommand(this);
            _restoreCommand = new RestoreCommand(this);
            _deleteCommand = new DeleteMessageCommand(this);
            _composeCommand = new ComposeCommand(this);
            _markAsSeenCommand = new MarkAsSeenCommand(this);
            _markAsNotSeenCommand = new MarkAsNotSeenCommand(this);
            _deleteContactCommand = new RelayCommand(OnDeleteContact);
            _createAccountCommand = new RelayCommand(OnCreateAccount);
            _configAccountCommand = new RelayCommand(OnConfigAccount);
            _createContactCommand = new RelayCommand(OnCreateContact);
            _refreshKeysCommand = new RelayCommand(OnRefreshKeys);
            _resetZoomCommand = new RelayCommand(p => Zoom = 100.0f);
            _selectAccountCommand = new RelayCommand(p => IsAccountSelectionRequested = true);

            Observable.Timer(TimeSpan.FromHours(24))
                .Subscribe(OnRefreshContactKeys);

            Observable.FromEventPattern(
                action => MessageSelectionChanged += action,
                action => MessageSelectionChanged -= action)
                .Throttle(TimeSpan.FromMilliseconds(100))
                .ObserveOn(new DispatcherSynchronizationContext(Application.Current.Dispatcher))
                .Subscribe(OnMessageSelectionCommitted);

            Observable.FromEventPattern<QueryStringEventArgs>(
                action => QueryStringChanged += action,
                action => QueryStringChanged -= action)
                .Select(x => x.EventArgs)
                .Where(x => (x.Text.Length > 2 || string.IsNullOrEmpty(x.Text)))
                .Throttle(TimeSpan.FromMilliseconds(200))
                .Select(x => x.Text)
                .Subscribe(OnQueryReceived);

            _outboxTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _outboxTimer.Tick += OnOutboxTimerTick;

            _contacts = new ObservableCollection<MailContactContext>();
            _contacts.CollectionChanged += (sender, e) => RaisePropertyChanged(() => Contacts);

            _zoom = 1.0f;
            _showAllMessages = true;
        }

        private void OnMessagesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            RaisePropertyChanged(() => Messages);
            RaisePropertyChanged(() => MessageCount);
        }

        private async void OnRefreshKeys(object obj) {
            await RefreshKeysForAllContactsAsync();
        }

        private async void OnRefreshContactKeys(long obj) {
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
            } catch (Exception ex) {
                Logger.Error(ex);
            } finally {
                StatusText = Resources.ReadyStatus;
            }
        }

        private async Task UpdateKeysInDatabaseForContactAsync(MailContactModel contact, KeyCollection collection) {
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
            var address = string.Format("{0}/keys?email={1}", server, contact.Address);

            var uri = new Uri(address, UriKind.Absolute);
            client.Headers.Add(HttpRequestHeader.UserAgent, Settings.Default.UserAgent);
            var response = await client.OpenReadTaskAsync(uri);

            using (var reader = new StreamReader(response)) {
                var json = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<KeyCollection>(json);
            }
        }

        private void OnDeleteContact(object obj) {

        }

        internal async Task LoadContactsAsync() {
            IEnumerable<MailContactModel> contacts;
            using (var database = new DatabaseContext()) {
                contacts = await database.MailContacts.ToArrayAsync();
            }

            _contacts.AddRange(contacts.Select(x => new MailContactContext(x)));
        }

        /// <summary>
        ///     Queries the message source. This method must be called on a background thread.
        /// </summary>
        /// <param name="source">The message source to query.</param>
        /// <returns>Returns an async task.</returns>
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

        public event EventHandler MailboxSelectionChanged;

        private void OnMailboxSelectionChanged() {
            try {
                var handler = MailboxSelectionChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);

                if (SelectedMailbox == null) {
                    return;
                }

                Task.Run((Action)RefreshViewForSelectedMailbox);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private async void RefreshViewForSelectedMailbox() {
            await RequestMessagesAsync(SelectedMailbox);
            await SelectedMailbox.SyncMessagesAsync();
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
        }

        private void RefreshContactStatisticsAsync(IEnumerable<MailContactContext> contacts = null) {
            var items = (contacts ?? _contacts).ToArray();
            Task.Factory.StartNew(() => {
                foreach (var tasks in items.Select(item => new[] {
                    item.CountNotSeenAsync(),
                    item.CountMessagesAsync()
                })) {
                    Task.WaitAll(tasks);
                }
            });
        }

        internal event EventHandler<QueryStringEventArgs> QueryStringChanged;

        private void OnQueryStringChanged(QueryStringEventArgs e) {
            var handler = QueryStringChanged;
            if (handler != null)
                handler(this, e);
        }

        private async void RefreshViewChangedQueryString(string query) {
            if (string.IsNullOrWhiteSpace(query)) {
                await RequestMessagesAsync(SelectedMailbox);
            } else {
                await RequestMessagesAsync(new QueryContext(query));
            }
        }

        #endregion

        #region Property Declarations

        public IEnumerable<MailContactContext> Contacts {
            get { return _contacts; }
        }

        public ICollection<AttachmentContext> Attachments {
            get {
                return _attachments;
            }
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

        public bool IsAllContactsSelected {
            get { return _isAllContactsSelected; }
            set {
                if (_isAllContactsSelected == value) {
                    return;
                }
                _isAllContactsSelected = value;
                RaisePropertyChanged(() => IsAllContactsSelected);
                OnIsAllContactsSelected();
            }
        }

        private void OnIsAllContactsSelected() {
            if (IsAllContactsSelected && SelectedContact != null) {
                Contacts.ForEach(x => x.IsSelected = false);
            } else {
                OnContactSelectionChanged();
            }
        }

        public MailContactContext SelectedContact {
            get { return _selectedContact; }
            set {
                if (_selectedContact == value) {
                    return;
                }
                _selectedContact = value;
                RaisePropertyChanged(() => SelectedContact);
                OnContactSelectionChanged();
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

        private void OnContactSelectionChanged() {

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

        private void OnMessageFilterChanged() {
            var mailbox = SelectedMailbox;
            if (mailbox == null) {
                return;
            }

            mailbox.ShowAllMessages = ShowAllMessages;
            Task.Run(() => RequestMessagesAsync(mailbox));
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
                if (Math.Abs(_zoom - value) < float.Epsilon) {
                    return;
                }
                _zoom = value;
                RaisePropertyChanged(() => Zoom);
            }
        }

        public ICommand ConfigAccountCommand {
            get { return _configAccountCommand; }
        }

        public ICommand SelectAccountCommand {
            get { return _selectAccountCommand; }
        }

        public ICommand ResetZoomCommand {
            get { return _resetZoomCommand; }
        }

        public ICommand PrintCommand {
            get { return _printCommand; }
        }

        public ICommand RefreshKeysCommand {
            get { return _refreshKeysCommand; }
        }

        public ICommand CreateContactCommand {
            get { return _createContactCommand; }
        }

        public ICommand ComposeCommand {
            get { return _composeCommand; }
        }

        public ICommand RestoreCommand {
            get { return _restoreCommand; }
        }
        public ICommand ReplyCommand {
            get { return _replyCommand; }
        }

        public ICommand DeleteContactCommand {
            get { return _deleteContactCommand; }
        }

        public ICommand ForwardCommand {
            get { return _forwardCommand; }
        }

        public ICommand DeleteMessageCommand {
            get { return _deleteCommand; }
        }

        public ICommand MarkAsSeenCommand {
            get { return _markAsSeenCommand; }
        }

        public ICommand MarkAsNotSeenCommand {
            get { return _markAsNotSeenCommand; }
        }

        public ICommand CreateAccountCommand {
            get { return _createAccountCommand; }
        }

        public PublicKeyCrypto KeyContainer { get; private set; }

        public IEnumerable<MailMessageContext> SelectedMessages {
            get { return _messages.Where(x => x.IsSelected).ToArray(); }
        }

        public MailMessageContext SelectedMessage {
            get {
                return SelectedMessages == null
                    ? null
                    : SelectedMessages.FirstOrDefault();
            }
        }

        public bool IsAccountSelectionRequested {
            get { return _isAccountSelectionRequested; }
            set {
                if (_isAccountSelectionRequested == value) {
                    return;
                }

                _isAccountSelectionRequested = value;
                RaisePropertyChanged(() => IsAccountSelectionRequested);
            }
        }

        #endregion

        private void OnQueryReceived(string text) {
            Task.Run(() => RefreshViewChangedQueryString(text));
        }

        private void OnAccountsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            RaisePropertyChanged(() => Accounts);
        }

        private void OnMessageSelectionCommitted(EventPattern<object> obj) {
            Application.Current.AssertUIThread();

            ClearPreviewArea();
            var message = SelectedMessages.FirstOrDefault();
            if (message == null) {
                return;
            }

            Task.Run(() => RefreshMessageSelectionAsync(message));
        }

        private async void RefreshMessageSelectionAsync(MailMessageContext message) {
            await MarkSelectionAsSeenAsync();
            if (!await message.GetIsMimeLoadedAsync()) {
                await message.DownloadMessageAsync();
            }
            await PreviewMessageAsync(message);
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

                var reader = new MailMessageReader(Encoding.UTF8.GetBytes(mimeMessage.Data));
                var attachments = reader.FindAllAttachments();

                attachments.Where(x => x.ContentId == null)
                    .ForEach(y => attachmentContexts.Add(new AttachmentContext(y)));
            }
            Application.Current.Dispatcher.Invoke(() => {
                Attachments.Clear();
                Attachments.AddRange(attachmentContexts);
            });
        }

        private Task PreviewMessageAsync(MailMessageContext message) {
            Source = string.Format("asset://paranoia/message/{0}", message.Id);
            return DisplayAttachmentAsync(message);
        }

        internal static DirectoryInfo GetKeyDirectory() {
            var dataDir = (string)AppDomain.CurrentDomain.GetData("DataDirectory");
            return new DirectoryInfo(Path.Combine(dataDir, "keys"));
        }

        public async Task RunAsync() {
            await LoadContactsAsync();
            await LoadAccountsAsync();
            SyncAccountsAsync();

            _outboxTimer.Start();
        }

        private async Task LoadAccountsAsync() {
            using (var context = new DatabaseContext()) {
                var accounts = await context.MailAccounts.ToArrayAsync();
                _accounts.AddRange(accounts.Select(x => new MailAccountContext(x)));
            }

            foreach (var account in Accounts) {
                await account.LoadMailboxesAsync();
            }
        }

        private void SyncAccountsAsync() {
            foreach (var account in Accounts) {
                var a = account;
                Task.Run(() => a.SyncMailboxesAsync());
            }
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

        internal void OnComposeMessage() {
            var uri = typeof(ComposeMessagePage).ToPageUri("?action=new");
            OnFlyOutNavigationRequested(new NavigationRequestedEventArgs(uri));
        }

        private void OnCreateAccount(object obj) {
            var uri = typeof(CreateAccountPage).ToPageUri();
            OnFlyOutNavigationRequested(new NavigationRequestedEventArgs(uri));
        }

        internal void OnReplyToMessage() {
            if (SelectedMessage == null) {
                return;
            }
            var uri = typeof(ComposeMessagePage).ToPageUriAsReply(SelectedMessage);
            OnFlyOutNavigationRequested(new NavigationRequestedEventArgs(uri));
        }

        internal async void OpenDecryptKeyPairDialog() {
            var info = GetKeyDirectory();
            var publicKey = Path.Combine(info.FullName, Settings.Default.PublicKeyFile);
            var privateKey = Path.Combine(info.FullName, Settings.Default.PrivateKeyFile);

            KeyContainer = new PublicKeyCrypto();
            await KeyContainer.InitFromFileAsync(publicKey, privateKey);
            await App.Context.RunAsync();
        }

        private void OnConfigAccount(object obj) {
            var uri = typeof(AccountDetailsPage).ToPageUri();
            OnFlyOutNavigationRequested(new NavigationRequestedEventArgs(uri));
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

            await ProcessOutgoingMessagesAsync();
        }

        private Task ProcessOutgoingMessagesAsync() {
            var tasks = Accounts.Select(x => x.Outbox.ProcessOutgoingMessagesAsync());
            return Task.WhenAll(tasks);
        }

        internal async Task NotifyOutboxNotEmpty() {
            await ProcessOutgoingMessagesAsync();
        }

        public void ResetStatusText() {
            StatusText = Resources.ReadyStatus;
        }

        internal void NotifyContactsAdded(ICollection<MailContactContext> contacts) {
            Application.Current.AssertUIThread();

            _contacts.AddRange(contacts);
            RefreshContactStatisticsAsync(contacts);
        }

        internal void NotifyMessagesAdded(ICollection<MailMessageContext> messages) {
            foreach (var message in messages.Where(message => message.Mailbox.IsSelected)) {
                _messages.Add(message);
            }

            RefreshContactStatisticsAsync();
            RaisePropertyChanged(() => Messages);
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
            RaisePropertyChanged(() => Messages);
        }
    }
}
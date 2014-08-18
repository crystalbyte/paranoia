#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Composition;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI;
using Crystalbyte.Paranoia.UI.Commands;
using Crystalbyte.Paranoia.Contexts;
using Crystalbyte.Paranoia.UI.Pages;
using System.Windows;
using System.Reactive.Concurrency;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Crystalbyte.Paranoia.Net;
using System.Security.Principal;
using System.Security.AccessControl;
using Crystalbyte.Paranoia.Cryptography;

#endregion

namespace Crystalbyte.Paranoia {
    [Export, Shared]
    public sealed class AppContext : NotificationObject {

        #region Private Fields

        private float _zoom;
        private string _queryString;
        private string _source;
        private string _statusText;
        private bool _isPopupVisible;
        private bool _isAccountSelectionRequested;
        private MailAccountContext _transitAccount;
        private readonly DispatcherTimer _outboxTimer;
        private MailAccountContext _selectedAccount;
        private IEnumerable<MailMessageContext> _selectedMessages;
        private readonly ObservableCollection<MailMessageContext> _messages;
        private readonly ObservableCollection<MailAccountContext> _accounts;
        private readonly ObservableCollection<MailContactContext> _contacts;
        private MailContactContext _selectedContact;
        private readonly ICommand _printCommand;
        private readonly ICommand _resetZoomCommand;
        private readonly ICommand _replyCommand;
        private readonly ICommand _deleteCommand;
        private readonly ICommand _writeCommand;
        private readonly ICommand _forwardCommand;
        private readonly ICommand _markAsSeenCommand;
        private readonly ICommand _markAsNotSeenCommand;
        private readonly ICommand _configAccountCommand;
        private readonly ICommand _createAccountCommand;
        private readonly ICommand _selectAccountCommand;
        private readonly ICommand _deleteAccountCommand;
        private readonly ICommand _createContactCommand;
        private readonly ICommand _deleteContactCommand;
        private readonly ICommand _refreshKeysCommand;

        #endregion

        #region Construction

        public AppContext() {
            _accounts = new ObservableCollection<MailAccountContext>();
            _accounts.CollectionChanged += OnAccountsCollectionChanged;

            _messages = new ObservableCollection<MailMessageContext>();

            _printCommand = new PrintCommand(this);
            _replyCommand = new ReplyCommand(this);
            _forwardCommand = new ForwardCommand(this);
            _deleteCommand = new DeleteMessageCommand(this);
            _writeCommand = new ComposeMessageCommand(this);
            _markAsSeenCommand = new MarkAsSeenCommand(this);
            _markAsNotSeenCommand = new MarkAsNotSeenCommand(this);
            _deleteAccountCommand = new RelayCommand(OnDeleteAccount);
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
                .Subscribe(OnMessageSelectionCommitted);

            Observable.FromEventPattern<QueryStringEventArgs>(
                    action => QueryStringChanged += action,
                    action => QueryStringChanged -= action)
                .Select(x => x.EventArgs)
                .Where(x => (x.Text.Length > 2 || string.IsNullOrEmpty(x.Text))
                            && string.Compare(x.Text, Resources.SearchBoxWatermark,
                                StringComparison.InvariantCultureIgnoreCase) != 0)
                .Throttle(TimeSpan.FromMilliseconds(200))
                .Select(x => x.Text)
                .Subscribe(OnQueryReceived);

            _outboxTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _outboxTimer.Tick += OnOutboxTimerTick;

            _contacts = new ObservableCollection<MailContactContext>();
            _contacts.CollectionChanged += (sender, e) => RaisePropertyChanged(() => Contacts);
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
                Debug.WriteLine(ex.Message);
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
            } catch (Exception) {
                throw;
            }
        }

        private async Task<KeyCollection> DownloadKeysForContactAsync(MailContactModel contact, WebClient client) {
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

        internal async Task LoadContactsFromDatabaseAsync() {
            IEnumerable<MailContactModel> contacts;
            using (var database = new DatabaseContext()) {
                contacts = await database.MailContacts.ToArrayAsync();
            }

            _contacts.AddRange(contacts.Select(x => new MailContactContext(x)));
        }

        private async void OnDeleteAccount(object obj) {
            try {
                // TODO: Change into popup overlay.
                if (MessageBox.Show(Application.Current.MainWindow, Resources.DeleteAccountQuestion,
                    "Delete Account", MessageBoxButton.YesNo) == MessageBoxResult.No) {
                    return;
                }

                var account = SelectedAccount;
                await account.DeleteAsync();
                _accounts.Remove(account);

                if (_accounts.Count > 0) {
                    SelectedAccount = Accounts.First();
                }
            } catch (Exception) {
                throw;
            }
        }

        #endregion

        #region Public Events

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
        private void OnMessageSelectionChanged() {
            var handler = MessageSelectionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        internal event EventHandler AccountSelectionChanged;
        private async void OnAccountSelectionChanged() {
            var handler = AccountSelectionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);

            await RefreshContactStatisticsAsync();
        }

        private async Task RefreshContactStatisticsAsync(IEnumerable<MailContactContext> contacts = null) {
            var tasks = (contacts ?? _contacts).Select(x => x.CountNotSeenAsync());
            await Task.WhenAll(tasks);

            var tasks2 = (contacts ?? _contacts).Select(x => x.CountMessagesAsync());
            await Task.WhenAll(tasks2);
        }

        internal event EventHandler<QueryStringEventArgs> QueryStringChanged;
        private void OnQueryStringChanged(QueryStringEventArgs e) {
            var handler = QueryStringChanged;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        #region Property Declarations

        public IEnumerable<MailContactContext> Contacts {
            get { return _contacts; }
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

        public bool IsAccountSelected {
            get { return _selectedAccount != null; }
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

        public MailAccountContext SelectedAccount {
            get { return _selectedAccount; }
            set {
                if (_selectedAccount == value) {
                    return;
                }

                _selectedAccount = value;
                RaisePropertyChanged(() => SelectedAccount);
                RaisePropertyChanged(() => IsAccountSelected);
                OnAccountSelectionChanged();
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

        private async void OnContactSelectionChanged() {
            var account = SelectedAccount;
            if (account == null) {
                return;
            }

            var mailbox = account.SelectedMailbox;
            if (mailbox == null) {
                mailbox = account.Mailboxes.FirstOrDefault(x => x.Type == MailboxType.Inbox);
                account.SelectedMailbox = mailbox;
            }

            ClearMessages();

            // Clear all if still null;
            if (mailbox == null) {
                return;
            }

            var contact = SelectedContact;
            await mailbox.DisplayMessagesForContactAsync(contact);

            if (contact != null) {
                await contact.CountNotSeenAsync();
            }

            foreach (var box in account.Mailboxes.AsParallel()) {
                await box.CountNotSeenAsync();
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

        public bool CanSwitchAccounts {
            get { return _accounts != null && _accounts.Count > 1; }
        }

        public ICommand ConfigAccountCommand {
            get { return _configAccountCommand; }
        }

        public ICommand DeleteAccountCommand {
            get { return _deleteAccountCommand; }
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

        public ICommand WriteMessageCommand {
            get { return _writeCommand; }
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
            get { return _selectedMessages; }
            set {
                if (Equals(_selectedMessages, value)) {
                    return;
                }
                _selectedMessages = value;
                RaisePropertyChanged(() => SelectedMessage);
                RaisePropertyChanged(() => SelectedMessages);
                RaisePropertyChanged(() => IsMessageSelected);
                OnMessageSelectionChanged();
            }
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

        private async void OnQueryReceived(string text) {
            var mailbox = SelectedAccount.SelectedMailbox;
            if (string.IsNullOrEmpty(text)) {
                DisplayMessages(mailbox.Messages);
                return;
            }

            var messages = await mailbox.QueryAsync(text);
            DisplayMessages(messages);
        }

        private void OnAccountsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            RaisePropertyChanged(() => CanSwitchAccounts);
        }

        private async void OnMessageSelectionCommitted(EventPattern<object> obj) {
            ClearMessageView();
            var message = SelectedMessages.FirstOrDefault();
            if (message == null) {
                return;
            }

            await MarkSelectionAsSeenAsync();
            if (!await message.GetIsMimeLoadedAsync()) {
                await message.DownloadMessageAsync();
            }

            DisplayMessage(message);
        }

        internal async Task MarkSelectionAsSeenAsync() {
            var tasks = SelectedMessages
                .Where(x => x.IsNotSeen)
                .GroupBy(x => x.Mailbox)
                .Select(x => x.Key.MarkAsSeenAsync(x.ToArray()));

            await Task.WhenAll(tasks);
        }

        internal async Task MarkSelectionAsNotSeenAsync() {
            var tasks = SelectedMessages
                .Where(x => x.IsSeen)
                .GroupBy(x => x.Mailbox)
                .Select(x => x.Key.MarkAsNotSeenAsync(x.ToArray()));

            await Task.WhenAll(tasks);
        }

        internal void ClosePopup() {
            var uri = typeof(BlankPage).ToPageUri();
            OnPopupNavigationRequested(new NavigationRequestedEventArgs(uri));
            IsPopupVisible = false;
        }

        internal void DisplayMessages(ICollection<MailMessageContext> messages) {
            _messages.Clear();
            _messages.AddRange(messages);

            if (messages.Count > 0) {
                messages.OrderByDescending(x => x.EntryDate)
                    .First().IsSelected = true;
            }
        }

        private void ClearMessageView() {
            Source = null;
        }

        private void DisplayMessage(MailMessageContext message) {
            Source = string.Format("asset://paranoia/message/{0}", message.Id);
        }

        internal static DirectoryInfo GetKeyDirectory() {
            var dataDir = (string)AppDomain.CurrentDomain.GetData("DataDirectory");
            return new DirectoryInfo(Path.Combine(dataDir, "keys"));
        }


        public async Task RunAsync() {
            await LoadAccountsFromDatabaseAsync();
            await LoadContactsFromDatabaseAsync();
            SelectedAccount = Accounts.FirstOrDefault();
            if (SelectedAccount != null)
                SelectedAccount.IsSelected = true;

            _outboxTimer.Start();
        }

        private async Task LoadAccountsFromDatabaseAsync() {
            using (var context = new DatabaseContext()) {
                var accounts = await context.MailAccounts.ToArrayAsync();
                _accounts.AddRange(accounts.Select(x => new MailAccountContext(x)));
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
            var uri = typeof(ComposeMessagePage).ToPageUri();
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

        internal void ClearMessages() {
            _messages.Clear();
            ClearMessageView();
        }

        public void NotifyMessageCountChanged() {
            RaisePropertyChanged(() => Messages);
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

        internal async void NotifyContactsAdded(ICollection<MailContactContext> contacts) {
            _contacts.AddRange(contacts);
            await RefreshContactStatisticsAsync(contacts);
        }

        internal async void NotifyMessagesAdded(IEnumerable<MailMessageContext> messages) {
            _messages.AddRange(messages);
            await RefreshContactStatisticsAsync();
        }
    }
}
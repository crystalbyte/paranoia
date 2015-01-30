#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Composition;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Crystalbyte.Paranoia.Data;
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
        private bool _showAllMessages;
        private string _queryString;
        private string _source;
        private string _statusText;
        private bool _isPopupVisible;
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
        private readonly ICommand _deleteContactsCommand;
        private readonly ICommand _deleteMessagesCommand;
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

            _navigationOptions = new ObservableCollection<NavigationContext> {
                new NavigationContext { Title = Resources.MessagesTitle, TargetUri = typeof(MessagesPage).ToPageUri(), IsSelected = true},
                new NavigationContext { Title = Resources.ContactsTitle, TargetUri = typeof(ContactsPage).ToPageUri() }
            };

            _restoreMessagesCommand = new RestoreMessagesCommand(this);
            _markAsSeenCommand = new MarkAsSeenCommand(this);
            _blockContactsCommand = new BlockContactsCommand(this);
            _deleteMessagesCommand = new DeleteMessagesCommand(this);
            _markAsNotSeenCommand = new MarkAsNotSeenCommand(this);
            _deleteContactsCommand = new DeleteContactsCommand(this);
            _createAccountCommand = new RelayCommand(OnCreateAccount);
            _resetZoomCommand = new RelayCommand(p => Zoom = 100.0f);
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

            _zoom = 1.0f;
            _showAllMessages = true;
        }

        private async void OnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e) {
            if (!e.IsAvailable)
                return;

            await Application.Current.Dispatcher.InvokeAsync(async () => {
                foreach (var account in Accounts) {
                    await account.TakeOnlineAsync();
                }
            });
        }

        private async void OnContactQueryReceived(string query) {
            await FilterContactsAsync(query);
        }

        private void OnMessagesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            RaisePropertyChanged(() => Messages);
            RaisePropertyChanged(() => MessageCount);
            RaisePropertyChanged(() => IsMessageOrSmtpRequestSelected);
        }

        public object Alphabet {
            get {
                return Enumerable.Range(65, 26)
                    .Select(Convert.ToChar).ToArray();
            }
        }


        internal async Task FilterContactsAsync(string query) {
            Application.Current.AssertUIThread();

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
            Application.Current.AssertBackgroundThread();

            IEnumerable<MailContactModel> contacts;
            using (var database = new DatabaseContext()) {
                contacts = await database.MailContacts.ToArrayAsync();
            }

            await Application.Current.Dispatcher.InvokeAsync(() =>
                _contacts.AddRange(contacts.Select(x => new MailContactContext(x))));

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
            Application.Current.AssertUIThread();

            _messages.Clear();

            source.IsLoadingMessages = true;
            var messages = await Task.Run(() => source.GetMessagesAsync());
            source.IsLoadingMessages = false;

            // User might have switched to a different mailbox by now.
            if (SelectedMailbox != source && !(source is QueryContext)) {
                return;
            }

            _messages.DeferNotifications = true;
            _messages.AddRange(messages);
            _messages.DeferNotifications = false;
            _messages.NotifyCollectionChanged();
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
            Application.Current.AssertUIThread();

            await RequestOutboxContentAsync(SelectedOutbox);
        }

        private async Task RefreshViewForSelectedMailboxAsync() {
            Application.Current.AssertUIThread();

            await RequestMessagesAsync(SelectedMailbox);
            if (!SelectedMailbox.IsSyncingMessages) {
                await SelectedMailbox.SyncMessagesAsync();
            }
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

        private async Task RefreshViewChangedQueryString(string query) {
            Application.Current.AssertUIThread();

            if (SelectedMailbox == null) {
                return;
            }

            if (String.IsNullOrWhiteSpace(query)) {
                await RequestMessagesAsync(SelectedMailbox);
            } else {
                await RequestMessagesAsync(new QueryContext(query));
            }
        }

        private static async Task RequestOutboxContentAsync(OutboxContext outbox) {
            await outbox.LoadSmtpRequestsAsync();
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

        public ICollection<AttachmentContext> Attachments {
            get { return _attachments; }
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
            try {
                var mailbox = SelectedMailbox;
                if (mailbox == null) {
                    return;
                }

                mailbox.ShowAllMessages = ShowAllMessages;
                await RequestMessagesAsync(mailbox);
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

        internal async Task RefreshMessageSelectionAsync(MailMessageContext message) {
            Application.Current.AssertUIThread();

            await MarkSelectionAsSeenAsync();
            await Task.Run(async () => {
                if (!await message.GetIsMimeLoadedAsync()) {
                    await message.DownloadMessageAsync();
                }
                await message.UpdateTrustLevelAsync();
            });

            message.InvalidateBindings();

            if (message.IsSourceTrusted) {
                await ViewUnblockedMessageAsync(message);
            } else {
                await ViewMessageAsync(message);
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

        private void ClearPreviewArea() {
            Application.Current.AssertUIThread();

            _attachments.Clear();
            Source = null;
        }

        private async Task DisplayAttachmentAsync(MailMessageContext message) {
            Application.Current.AssertUIThread();

            var attachmentContexts = new List<AttachmentContext>();
            await Task.Run(async () => {
                using (var context = new DatabaseContext()) {
                    var mimeMessage = await context.MimeMessages.FirstOrDefaultAsync(x => x.MessageId == message.Id);
                    if (mimeMessage == null)
                        return;

                    var reader = new MailMessageReader(mimeMessage.Data);
                    var attachments = reader.FindAllAttachments();

                    attachments.Where(x => x.ContentId == null)
                        .ForEach(y => attachmentContexts.Add(new AttachmentContext(y)));
                }
            });

            Attachments.Clear();
            Attachments.AddRange(attachmentContexts);
        }

        private async Task ViewMessageAsync(MailMessageContext message) {
            Source = string.Format("asset://paranoia/message/{0}", message.Id);
            await DisplayAttachmentAsync(message);
        }

        private async Task ViewUnblockedMessageAsync(MailMessageContext message) {
            Source = string.Format("asset://paranoia/message/{0}?blockExternals=false", message.Id);
            await DisplayAttachmentAsync(message);
        }

        public async Task RunAsync() {
            Application.Current.AssertUIThread();

            await Task.Run(async () => {
                await LoadContactsAsync();
                await LoadAccountsAsync();
            });

            foreach (var account in Accounts) {
                await account.TakeOnlineAsync();
            }

            _outboxTimer.Start();
        }

        private async Task LoadAccountsAsync() {
            Application.Current.AssertBackgroundThread();

            IEnumerable<MailAccountModel> accounts;
            using (var context = new DatabaseContext()) {
                accounts = await context.MailAccounts.ToArrayAsync();
            }

            await Application.Current.Dispatcher.InvokeAsync(() =>
                    _accounts.AddRange(accounts.Select(x => new MailAccountContext(x))));

            foreach (var account in Accounts) {
                await account.LoadMailboxesAsync();
                await account.Outbox.CountSmtpRequestsAsync();
            }
        }

        internal void CreateMailbox(IMailboxCreator parent) {
            NavigationArguments.Push(parent);

            var uri = typeof(CreateMailboxModalPage).ToPageUri();
            OnModalNavigationRequested(new NavigationRequestedEventArgs(uri));
            IsPopupVisible = true;
        }

        private void OnCreateAccount(object obj) {
            var account = new MailAccountContext(new MailAccountModel());
            NavigationArguments.Push(account);

            var uri = typeof(CreateAccountStartFlyoutPage).ToPageUri();
            OnFlyoutNavigationRequested(new NavigationRequestedEventArgs(uri));
        }

        internal void CloseFlyout() {
            OnFlyoutClosing();
            OnFlyoutCloseRequested();
            OnFlyoutClosed();
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
            var uri = typeof(CompositionPage).ToPageUri(url);
            App.Context.Compose(uri);
        }

        internal void Reply(FileSystemInfo file) {
            var path = Uri.EscapeDataString(file.FullName);
            var url = string.Format("?action=reply&path={0}", path);
            var uri = typeof(CompositionPage).ToPageUri(url);
            App.Context.Compose(uri);
        }

        internal void Forward() {
            Forward(SelectedMessage);
        }

        internal void Forward(MailMessageContext message) {
            var url = string.Format("?action=forward&id={0}", message.Id);
            var uri = typeof(CompositionPage).ToPageUri(url);
            App.Context.Compose(uri);
        }

        internal void Forward(FileSystemInfo file) {
            var path = Uri.EscapeDataString(file.FullName);
            var url = string.Format("?action=forward&path={0}", path);
            var uri = typeof(CompositionPage).ToPageUri(url);
            App.Context.Compose(uri);
        }

        internal void ReplyToAll() {
            ReplyToAll(SelectedMessage);
        }

        internal void ReplyToAll(MailMessageContext message) {
            var url = string.Format("?action=reply-all&id={0}", message.Id);
            var uri = typeof(CompositionPage).ToPageUri(url);
            App.Context.Compose(uri);
        }

        internal void ReplyToAll(FileSystemInfo file) {
            var path = Uri.EscapeDataString(file.FullName);
            var url = string.Format("?action=reply-all&path={0}", path);
            var uri = typeof(CompositionPage).ToPageUri(url);
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
            window.Source = typeof(CompositionPage).ToPageUri("?action=new");

            if (owner.WindowState == WindowState.Maximized) {
                window.WindowState = WindowState.Maximized;
            }

            window.Show();
        }

        internal async Task InspectMessageAsync(FileInfo file) {
            var owner = Application.Current.MainWindow;
            var inspector = new InspectionWindow();
            inspector.MimicOwnership(owner);

            if (owner.WindowState == WindowState.Maximized) {
                inspector.WindowState = WindowState.Maximized;
            }

            await inspector.InitWithFileAsync(file);
            inspector.Show();
        }
        internal async Task InspectMessageAsync(MailMessageContext message) {
            var owner = Application.Current.MainWindow;
            var inspector = new InspectionWindow();
            inspector.MimicOwnership(owner);

            if (owner.WindowState == WindowState.Maximized) {
                inspector.WindowState = WindowState.Maximized;
            }

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

        internal void NotifyMessagesRemoved(IEnumerable<long> ids) {
            foreach (var message in ids.Select(id => _messages.FirstOrDefault(x => x.Id == id)).Where(message => message != null)) {
                _messages.Remove(message);
            }
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

        public void NotifySeenStatesChanged(IDictionary<long, MailMessageModel> messages) {
            foreach (var message in from message in _messages let hasKey = messages.ContainsKey(message.Id) where hasKey select message) {
                message.IsSeen = messages[message.Id].HasFlag(MailMessageFlags.Seen);
            }
        }
    }
}
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
        private bool _showOnlyFavorites;
        private bool _showOnlyWithAttachments;
        private string _queryString;
        private string _source;
        private string _statusText;
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

        #endregion

        #region Construction

        public AppContext() {
            _accounts = new ObservableCollection<MailAccountContext>();
            _accounts.CollectionChanged += OnAccountsCollectionChanged;

            _messages = new DeferredObservableCollection<MailMessageContext>();
            _messages.CollectionChanged += OnMessagesCollectionChanged;

            _navigationOptions = new ObservableCollection<NavigationContext>
            {
                new NavigationContext
                {
                    Title = Resources.MessagesTitle,
                    TargetUri = typeof (MessagesPage).ToPageUri(),
                    IsSelected = true
                },
                new NavigationContext {Title = Resources.ContactsTitle, TargetUri = typeof (ContactsPage).ToPageUri()}
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

            OnItemSelectionRequested(new ItemSelectionRequestedEventArgs(SelectionPosition.First));
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

        private async void OnMailboxSelectionChanged() {
            try {
                var handler = MailboxSelectionChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);

                if (SelectedMailbox == null) {
                    return;
                }

                Clear();
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

        public bool ShowOnlyWithAttachments {
            get { return _showOnlyWithAttachments; }
            set {
                if (_showOnlyWithAttachments == value) {
                    return;
                }
                _showOnlyWithAttachments = value;
                RaisePropertyChanged(() => ShowOnlyWithAttachments);
                OnMessageFilterChanged();
            }
        }

        public bool ShowOnlyFavorites {
            get { return _showOnlyFavorites; }
            set {
                if (_showOnlyFavorites == value) {
                    return;
                }
                _showOnlyFavorites = value;
                RaisePropertyChanged(() => ShowOnlyFavorites);
                OnMessageFilterChanged();
            }
        }

        private async void OnMessageFilterChanged() {
            try {
                var mailbox = SelectedMailbox;
                if (mailbox == null) {
                    return;
                }

                await Application.Current.Dispatcher.InvokeAsync(() => {
                    mailbox.ShowAllMessages = ShowAllMessages;
                    mailbox.ShowOnlyFavorites = ShowOnlyFavorites;
                    mailbox.ShowOnlyWithAttachments =
                        ShowOnlyWithAttachments;
                });

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

            var message = SelectedMessages.FirstOrDefault();
            if (message == null) {
                return;
            }

            try {
                await ViewMessageAsync(message);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        internal async Task ViewMessageAsync(MailMessageContext message) {
            Application.Current.AssertUIThread();

            try {
                var mark = MarkAsSeenAsync(message);
                await Task.Run(async () => {
                    if (!await message.GetIsMimeLoadedAsync()) {
                        await message.FetchAndDecryptAsync();
                    }
                });

                if (!message.IsInitialized) {
                    await message.InitDetailsAsync();
                }

                Source = string.Format(message.IsExternalContentAllowed
                    ? "message:///{0}?blockExternals=false"
                    : "message:///{0}", message.Id);

                await mark;
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
            }
        }

        internal Task MarkAsSeenAsync(MailMessageContext message) {
            return Task.Run(async () => {
                var mailbox = message.Mailbox;
                await mailbox.MarkAsSeenAsync(new[] { message });
            });
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
                var pair = new KeyPairModel {
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

            if (!await CheckKeyPairAsync()) {
                await GenerateKeyPairAsync();
            }

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
            Application.Current.AssertUIThread();

            var messages = SelectedMessages.ToArray();

            try {
                var accountGroups = messages.GroupBy(x => x.Mailbox.Account).ToArray();
                foreach (var accountGroup in accountGroups) {
                    var trash = accountGroup.Key.GetTrashMailbox();
                    if (trash == null) {
                        throw new InvalidOperationException(Resources.MissingTrashFolderException);
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

        internal Task ReplyAsync() {
            return ReplyAsync(SelectedMessage);
        }

        internal async Task ReplyAsync(MailMessageContext message) {
            var id = message.Id.ToString(CultureInfo.InvariantCulture);
            var owner = Application.Current.MainWindow;
            var window = new CompositionWindow();
            await window.PrepareAsReplyAsync(new Dictionary<string, string>
            {
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
            await window.PrepareAsReplyAsync(new Dictionary<string, string>
            {
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
            OnItemSelectionRequested(new ItemSelectionRequestedEventArgs(SelectionPosition.Next, collection));
            collection.ForEach(x => _messages.Remove(x));
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
                        //c.Classification = ContactClassification.Spam;
                        //contact.IsIgnored = block;
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
            foreach (
                var message in
                    from message in _messages let hasKey = messages.ContainsKey(message.Id) where hasKey select message) {
                message.IsSeen = messages[message.Id].HasFlag(MailMessageFlags.Seen);
            }
        }
    }
}
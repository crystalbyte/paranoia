#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI.Commands;
using NLog;
using System.Windows;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class MailAccountContext : HierarchyContext {

        #region Private Fields

        private bool _isTesting;
        private bool _takeOnlineHint;
        private bool _isAutoDetectPreferred;
        private bool _isDetectingSettings;
        private TestingContext _testing;
        private bool _isManagingMailboxes;
        private bool _isMailboxSelectionAvailable;

        private readonly AppContext _appContext;
        private readonly MailAccountModel _account;
        private readonly ICommand _listMailboxesCommand;
        private readonly ICommand _testSettingsCommand;
        private readonly ICommand _deleteAccountCommand;
        private readonly ICommand _configAccountCommand;
        private readonly ICommand _showUnsubscribedMailboxesCommand;
        private readonly ICommand _hideUnsubscribedMailboxesCommand;
        private readonly OutboxContext _outbox;
        private readonly ObservableCollection<MailboxContext> _mailboxes;
        private bool _isSyncingMailboxes;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        internal MailAccountContext(MailAccountModel account) {
            _account = account;
            _appContext = App.Context;
            _takeOnlineHint = true;
            _outbox = new OutboxContext(this);
            _listMailboxesCommand = new RelayCommand(OnListMailboxes);
            _testSettingsCommand = new RelayCommand(OnTestSettings);
            _configAccountCommand = new RelayCommand(OnConfigAccount);
            _deleteAccountCommand = new RelayCommand(OnDeleteAccount);
            _showUnsubscribedMailboxesCommand = new RelayCommand(OnShowUnsubscribedMailboxes);
            _hideUnsubscribedMailboxesCommand = new RelayCommand(OnHideUnsubscribedMailboxes);
            _isAutoDetectPreferred = true;

            _mailboxes = new ObservableCollection<MailboxContext>();
            _mailboxes.CollectionChanged += (sender, e) => {
                RaisePropertyChanged(() => HasMailboxes);
                RaisePropertyChanged(() => Children);
                RaisePropertyChanged(() => MailboxRoots);
            };
        }

        private async void OnDeleteAccount(object obj) {
            try {
                if (MessageBox.Show(Application.Current.MainWindow,
                        Resources.DeleteAccountQuestion, Resources.ApplicationName, MessageBoxButton.YesNo) == MessageBoxResult.No) {
                    return;
                }
                App.Context.NotifyAccountDeleted(this);
                await Task.Run((Action)OnDeleteAccountAsync);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private async void OnDeleteAccountAsync() {
            var success = await TryDeleteAccountAsync();
            if (!success) {
                Application.Current.Dispatcher.Invoke(
                    () => App.Context.NotifyAccountCreated(this));
            }
        }

        public ICommand DeleteAccountCommand {
            get { return _deleteAccountCommand; }
        }


        private static void OnConfigAccount(object obj) {
            App.Context.ConfigureAccount(obj as MailAccountContext);
        }

        private void OnHideUnsubscribedMailboxes(object obj) {
            IsManagingMailboxes = false;
            Mailboxes.ForEach(x => x.IsEditing = false);
        }

        private void OnShowUnsubscribedMailboxes(object obj) {
            IsManagingMailboxes = true;
            Mailboxes.ForEach(x => x.IsEditing = true);
            IsExpanded = true;
        }

        #endregion

        public string TrashMailboxName {
            get { return _account.TrashMailboxName; }
            set {
                if (_account.TrashMailboxName == value) {
                    return;
                }
                _account.TrashMailboxName = value;
                RaisePropertyChanged(() => TrashMailboxName);
            }
        }

        public string SignaturePath {
            get { return _account.SignaturePath; }
            set {
                if (_account.SignaturePath == value) {
                    return;
                }
                _account.SignaturePath = value;
                RaisePropertyChanged(() => SignaturePath);
            }
        }

        public string JunkMailboxName {
            get { return _account.JunkMailboxName; }
            set {
                if (_account.JunkMailboxName == value) {
                    return;
                }
                _account.JunkMailboxName = value;
                RaisePropertyChanged(() => JunkMailboxName);
            }
        }

        public string SentMailboxName {
            get { return _account.SentMailboxName; }
            set {
                if (_account.SentMailboxName == value) {
                    return;
                }

                _account.SentMailboxName = value;
                RaisePropertyChanged(() => SentMailboxName);
            }
        }

        public string DraftMailboxName {
            get { return _account.DraftMailboxName; }
            set {
                if (_account.DraftMailboxName == value) {
                    return;
                }
                _account.DraftMailboxName = value;
                RaisePropertyChanged(() => DraftMailboxName);
            }
        }

        private void OnListMailboxes(object obj) {
            IsMailboxSelectionAvailable = true;
        }

        private async void OnTestSettings(object obj) {
            await TestSettingsAsync();
        }

        public ICommand ConfigAccountCommand {
            get { return _configAccountCommand; }
        }

        public ICommand TestSettingsCommand {
            get { return _testSettingsCommand; }
        }

        public ICommand ListMailboxesCommand {
            get { return _listMailboxesCommand; }
        }

        public ICommand HideUnsubscribedMailboxesCommand {
            get { return _hideUnsubscribedMailboxesCommand; }
        }

        public ICommand ShowUnsubscribedMailboxesCommand {
            get { return _showUnsubscribedMailboxesCommand; }
        }

        public bool IsMailboxSelectionAvailable {
            get { return _isMailboxSelectionAvailable; }
            set {
                if (_isMailboxSelectionAvailable == value) {
                    return;
                }
                _isMailboxSelectionAvailable = value;
                RaisePropertyChanged(() => IsMailboxSelectionAvailable);
            }
        }

        internal async Task TakeOnlineAsync() {
            Application.Current.AssertBackgroundThread();

            try {
                if (_mailboxes.Count == 0) {
                    await LoadMailboxesAsync();
                }

                await Application.Current.Dispatcher.InvokeAsync(async () => {
                    if (!IsSyncingMailboxes) {
                        await SyncMailboxesAsync();
                    }
                    var inbox = GetInbox();
                    if (inbox != null) {
                        await inbox.IdleAsync();
                    }
                });
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        internal async Task<List<ImapMailboxInfo>> ListMailboxesAsync(string pattern = "") {
            Application.Current.AssertBackgroundThread();

            using (var connection = new ImapConnection { Security = ImapSecurity }) {
                connection.RemoteCertificateValidationFailed += (sender, e) => e.IsCanceled = false;
                using (var auth = await connection.ConnectAsync(ImapHost, ImapPort)) {
                    using (var session = await auth.LoginAsync(ImapUsername, ImapPassword)) {

                        if (connection.HasNamespaces) {
                            await session.GetNamespacesAsync();
                        }

                        var wildcard = string.IsNullOrEmpty(pattern) ? "%" : pattern;
                        return await session.ListAsync("", ImapMailbox.EncodeName(wildcard));
                    }
                }
            }
        }

        internal async Task<List<ImapMailboxInfo>> ListSubscribedMailboxesAsync(string pattern = "") {
            Application.Current.AssertBackgroundThread();

            using (var connection = new ImapConnection { Security = ImapSecurity }) {
                connection.RemoteCertificateValidationFailed += (sender, e) => e.IsCanceled = false;
                using (var auth = await connection.ConnectAsync(ImapHost, ImapPort)) {
                    using (var session = await auth.LoginAsync(ImapUsername, ImapPassword)) {
                        var wildcard = string.IsNullOrEmpty(pattern) ? "%" : pattern;
                        return await session.LSubAsync("", ImapMailbox.EncodeName(wildcard));
                    }
                }
            }
        }

        public bool StoreCopiesOfSentMessages {
            get { return _account.StoreCopiesOfSentMessages; }
            set {
                if (_account.StoreCopiesOfSentMessages == value) {
                    return;
                }
                _account.StoreCopiesOfSentMessages = value;
                RaisePropertyChanged(() => StoreCopiesOfSentMessages);
            }
        }

        public bool IsSyncingMailboxes {
            get { return _isSyncingMailboxes; }
            set {
                if (_isSyncingMailboxes == value) {
                    return;
                }
                _isSyncingMailboxes = value;
                RaisePropertyChanged(() => IsSyncingMailboxes);
            }
        }

        internal async Task SyncMailboxesAsync() {
            Application.Current.AssertUIThread();

            if (IsSyncingMailboxes) {
                throw new InvalidOperationException();
            }

            IsSyncingMailboxes = true;

            var mailboxes = _mailboxes.ToArray();

            await Task.Run(async () => {
                try {
                    App.Context.StatusText = Resources.SyncMailboxesStatus;

                    Debug.WriteLine("**sync**");
                    var remoteMailboxes = await ListMailboxesAsync();
                    var subscribed = await ListSubscribedMailboxesAsync();

                    if (IsGmail) {
                        // Fetch gmail folders and assign automagically.
                        var gmail =
                            remoteMailboxes.FirstOrDefault(x =>
                                    x.Name.ContainsIgnoreCase("gmail") ||
                                    x.Name.ContainsIgnoreCase("google mail"));
                        if (gmail != null) {
                            var pattern = string.Format("{0}{1}%", gmail.Name, gmail.Delimiter);
                            var localizedMailboxes = await ListMailboxesAsync(pattern);
                            remoteMailboxes.AddRange(localizedMailboxes);
                        }
                    }

                    foreach (var mailbox in remoteMailboxes.Where(x => mailboxes.All(y =>
                        string.Compare(x.Fullname, y.Name,
                            StringComparison.InvariantCultureIgnoreCase) != 0))) {
                        var context = new MailboxContext(this, new MailboxModel {
                            AccountId = _account.Id
                        });

                        await context.InsertAsync();

                        if (mailbox.IsGmailAll) {
                            context.IsSubscribed = true;
                            goto done;
                        }

                        if (mailbox.IsGmailImportant) {
                            context.IsSubscribed = true;
                            goto done;
                        }

                        if (mailboxes.All(x => !x.IsJunk)) {
                            if (mailbox.IsGmailJunk || mailbox.Name.ContainsIgnoreCase("junk")) {
                                JunkMailboxName = mailbox.Fullname;
                                context.IsSubscribed = true;
                                goto done;
                            }
                        }

                        if (mailboxes.All(x => !x.IsDraft)) {
                            if (mailbox.IsGmailDraft ||
                                mailbox.Name.ContainsIgnoreCase("draft")) {
                                DraftMailboxName = mailbox.Fullname;
                                context.IsSubscribed = true;
                                goto done;
                            }
                        }

                        if (mailboxes.All(x => !x.IsSent)) {
                            if (mailbox.IsGmailSent || mailbox.Name.ContainsIgnoreCase("sent")) {
                                SentMailboxName = mailbox.Fullname;
                                context.IsSubscribed = true;
                                goto done;
                            }
                        }

                        if (mailboxes.All(x => !x.IsTrash)) {
                            if (mailbox.IsGmailTrash ||
                                mailbox.Name.ContainsIgnoreCase("trash")) {
                                TrashMailboxName = mailbox.Fullname;
                                context.IsSubscribed = true;
                            }
                        }

                    done:
                        await context.BindMailboxAsync(mailbox, subscribed);
                        await Application.Current.Dispatcher.InvokeAsync(() => _mailboxes.Add(context));
                    }

                    await SaveAsync();

                    await Application.Current.Dispatcher.InvokeAsync(() => {
                        var inbox = _mailboxes.FirstOrDefault(x => x.IsInbox);
                        if (inbox != null) {
                            inbox.IsSelected = true;
                        }
                    });


                } catch (Exception ex) {
                    Logger.Error(ex);
                }
            });

            App.Context.ResetStatusText();

            IsSyncingMailboxes = false;
        }

        internal async Task SaveAsync() {
            Application.Current.AssertBackgroundThread();

            using (var database = new DatabaseContext()) {
                database.MailAccounts.Attach(_account);
                database.Entry(_account).State = EntityState.Modified;
                await database.SaveChangesAsync();
            }
        }

        internal async Task LoadMailboxesAsync() {
            Application.Current.AssertBackgroundThread();

            MailboxModel[] models;
            using (var context = new DatabaseContext()) {
                models = await context.Mailboxes
                    .Where(x => x.AccountId == _account.Id)
                    .ToArrayAsync();
            }

            var mailboxes = new List<MailboxContext>(models.Select(x => new MailboxContext(this, x)));
            foreach (var mailbox in mailboxes) {
                await mailbox.CountNotSeenAsync();
            }

            await Application.Current.Dispatcher.InvokeAsync(() => {
                _mailboxes.AddRange(mailboxes);
                var inbox = _mailboxes.FirstOrDefault(x => x.IsInbox);
                if (inbox != null) {
                    inbox.IsSelected = true;
                }
            });
        }

        public string Address {
            get { return _account.Address; }
            set {
                if (_account.Address == value) {
                    return;
                }

                _account.Address = value;
                RaisePropertyChanged(() => Address);
            }
        }

        public Int64 Id {
            get { return _account.Id; }
        }

        public bool IsDetectingSettings {
            get { return _isDetectingSettings; }
            set {
                if (_isDetectingSettings == value) {
                    return;
                }
                _isDetectingSettings = value;
                RaisePropertyChanged(() => IsDetectingSettings);
            }
        }

        public bool IsGmail {
            get {
                return Settings.Default.GmailDomains
                    .OfType<string>()
                    .Any(x => _account.ImapHost
                        .EndsWith(x, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        public bool TakeOnlineHint {
            get { return _takeOnlineHint; }
            set {
                if (_takeOnlineHint == value) {
                    return;
                }

                _takeOnlineHint = value;
                RaisePropertyChanged(() => TakeOnlineHint);
            }
        }

        public OutboxContext Outbox {
            get { return _outbox; }
        }

        public bool IsManagingMailboxes {
            get { return _isManagingMailboxes; }
            set {
                if (_isManagingMailboxes == value) {
                    return;
                }

                _isManagingMailboxes = value;
                RaisePropertyChanged(() => IsManagingMailboxes);
            }
        }

        public AppContext AppContext {
            get { return _appContext; }
        }

        public bool HasMailboxes {
            get { return _mailboxes.Count > 0; }
        }

        public string Name {
            get { return _account.Name; }
            set {
                if (_account.Name == value) {
                    return;
                }

                _account.Name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        public string ImapHost {
            get { return _account.ImapHost; }
            set {
                if (_account.ImapHost == value) {
                    return;
                }

                _account.ImapHost = value;
                RaisePropertyChanged(() => ImapHost);
            }
        }

        public short ImapPort {
            get { return _account.ImapPort; }
            set {
                if (_account.ImapPort == value) {
                    return;
                }

                _account.ImapPort = value;
                RaisePropertyChanged(() => ImapPort);
            }
        }

        public string ImapUsername {
            get { return _account.ImapUsername; }
            set {
                if (_account.ImapUsername == value) {
                    return;
                }

                _account.ImapUsername = value;
                RaisePropertyChanged(() => ImapUsername);
            }
        }

        public string ImapPassword {
            get { return _account.ImapPassword; }
            set {
                if (_account.ImapPassword == value) {
                    return;
                }

                _account.ImapPassword = value;
                RaisePropertyChanged(() => ImapPassword);
            }
        }

        public SecurityProtocol ImapSecurity {
            get { return _account.ImapSecurity; }
            set {
                if (_account.ImapSecurity == value) {
                    return;
                }

                _account.ImapSecurity = value;
                RaisePropertyChanged(() => ImapSecurity);
            }
        }

        public string SmtpHost {
            get { return _account.SmtpHost; }
            set {
                if (_account.SmtpHost == value) {
                    return;
                }

                _account.SmtpHost = value;
                RaisePropertyChanged(() => SmtpHost);
            }
        }

        public TestingContext Testing {
            get { return _testing; }
            set {
                if (_testing == value) {
                    return;
                }
                _testing = value;
                RaisePropertyChanged(() => Testing);
            }
        }

        public short SmtpPort {
            get { return _account.SmtpPort; }
            set {
                if (_account.SmtpPort == value) {
                    return;
                }

                _account.SmtpPort = value;
                RaisePropertyChanged(() => SmtpPort);
            }
        }

        public string SmtpUsername {
            get { return _account.SmtpUsername; }
            set {
                if (_account.SmtpUsername == value) {
                    return;
                }

                _account.SmtpUsername = value;
                RaisePropertyChanged(() => SmtpUsername);
            }
        }

        public string SmtpPassword {
            get { return _account.SmtpPassword; }
            set {
                if (_account.SmtpPassword == value) {
                    return;
                }

                _account.SmtpPassword = value;
                RaisePropertyChanged(() => SmtpPassword);
            }
        }

        public SecurityProtocol SmtpSecurity {
            get { return _account.SmtpSecurity; }
            set {
                if (_account.SmtpSecurity == value) {
                    return;
                }

                _account.SmtpSecurity = value;
                RaisePropertyChanged(() => SmtpSecurity);
            }
        }

        public bool UseImapCredentialsForSmtp {
            get { return _account.UseImapCredentialsForSmtp; }
            set {
                if (_account.UseImapCredentialsForSmtp == value) {
                    return;
                }

                _account.UseImapCredentialsForSmtp = value;
                RaisePropertyChanged(() => UseImapCredentialsForSmtp);
            }
        }

        public bool IsAutoDetectPreferred {
            get { return _isAutoDetectPreferred; }
            set {
                if (_isAutoDetectPreferred == value) {
                    return;
                }
                _isAutoDetectPreferred = value;
                RaisePropertyChanged(() => IsAutoDetectPreferred);
            }
        }

        public bool IsTesting {
            get { return _isTesting; }
            set {
                if (_isTesting == value) {
                    return;
                }
                _isTesting = value;
                RaisePropertyChanged(() => IsTesting);
            }
        }

        public IEnumerable<MailboxContext> Mailboxes {
            get { return _mailboxes; }
        }

        public IEnumerable<MailboxContext> MailboxRoots {
            get {
                return _mailboxes.Where(x => !string.IsNullOrEmpty(x.Name) && !x.Name.Contains(x.Delimiter)).ToArray();
            }
        }

        public IEnumerable<object> Children {
            get {
                return _mailboxes
                    .Where(x => !string.IsNullOrEmpty(x.Name) && !x.Name.Contains(x.Delimiter))
                    .Cast<object>()
                    .Concat(new[] { Outbox })
                    .ToArray();
            }
        }

        internal async Task TestSettingsAsync() {
            IsTesting = true;

            await TestConnectivityAsync();
            if (Testing != null && Testing.IsFaulted) {
                IsTesting = false;
                return;
            }

            await TestImapSettingsAsync();
            if (Testing != null && Testing.IsFaulted) {
                IsTesting = false;
                return;
            }

            await TestSmtpSettingsAsync();
            if (Testing != null && Testing.IsFaulted) {
                IsTesting = false;
                return;
            }

            Testing = new TestingContext {
                Message = Resources.TestsCompletedSuccessfully
            };

            IsTesting = false;
        }

        private async Task TestSmtpSettingsAsync() {
            Testing = new TestingContext {
                Message = Resources.TestingSmtpStatus
            };

            try {
                using (var connection = new SmtpConnection { Security = SmtpSecurity }) {
                    using (var auth = await connection.ConnectAsync(SmtpHost, SmtpPort)) {
                        var username = UseImapCredentialsForSmtp ? ImapUsername : SmtpUsername;
                        var password = UseImapCredentialsForSmtp ? ImapPassword : SmtpPassword;
                        await auth.LoginAsync(username, password);
                    }
                }
            } catch (Exception ex) {
                Testing = new TestingContext {
                    IsFaulted = true,
                    Message = ex.Message,
                };

                Logger.Error(ex);
            }
        }

        private async Task TestImapSettingsAsync() {
            Testing = new TestingContext {
                Message = Resources.TestingImapStatus
            };

            try {
                using (var connection = new ImapConnection { Security = ImapSecurity }) {
                    using (var auth = await connection.ConnectAsync(ImapHost, ImapPort)) {
                        await auth.LoginAsync(ImapUsername, ImapPassword);
                    }
                }
            } catch (Exception ex) {
                Testing = new TestingContext {
                    IsFaulted = true,
                    Message = ex.Message
                };
                Logger.Error(ex);
            }
        }

        private async Task TestConnectivityAsync() {
            Testing = new TestingContext {
                Message = Resources.TestingConnectivityStatus
            };

            var available = false;
            await Task.Factory.StartNew(() => { available = NetworkInterface.GetIsNetworkAvailable(); });

            if (!available) {
                Testing = new TestingContext {
                    Message = Resources.TestingConnectivityStatus,
                    IsFaulted = true
                };
            }
        }

        internal async Task SaveSmtpRequestsAsync(IEnumerable<MailMessage> messages) {
            using (var database = new DatabaseContext()) {
                var account = await database.MailAccounts.FindAsync(_account.Id);

                foreach (var message in messages) {
                    var request = new SmtpRequestModel {
                        CompositionDate = DateTime.Now,
                        ToName = message.To.First().DisplayName,
                        ToAddress = message.To.First().Address,
                        Subject = message.Subject
                    };
                    var mime = await message.ToMimeAsync();
                    request.Mime = mime;

                    account.SmtpRequests.Add(request);
                }

                await database.SaveChangesAsync();
            }
        }

        internal async Task InsertAsync() {
            try {
                using (var database = new DatabaseContext()) {
                    database.MailAccounts.Add(_account);
                    await database.SaveChangesAsync();
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        public async Task DetectSettingsAsync() {
            var domain = Address.Split('@').Last();
            var url = string.Format("https://live.mozillamessaging.com/autoconfig/v1.1/{0}", domain);
            using (var client = new WebClient()) {
                try {
                    IsDetectingSettings = true;
                    var stream = await client.OpenReadTaskAsync(new Uri(url, UriKind.Absolute));
                    var serializer = new XmlSerializer(typeof(clientConfig));
                    var config = serializer.Deserialize(stream) as clientConfig;
                    Configure(config);
                } catch (WebException ex) {
                    Logger.Error(ex);
                    MakeEducatedGuess();
                } finally {
                    IsDetectingSettings = false;
                }
            }
        }

        private void MakeEducatedGuess() {
            // TODO: Yeah, guess bitch ...
        }

        private void Configure(clientConfig config) {
            if (!config.emailProvider.Any()) {
                return;
            }

            var provider = config.emailProvider.First();
            var imap = provider.incomingServer.FirstOrDefault(x => x.type.ContainsIgnoreCase("imap"));
            if (imap != null) {
                ImapHost = imap.hostname;
                ImapSecurity = imap.socketType.ToSecurityPolicy();
                ImapPort = short.Parse(imap.port);
                ImapUsername = GetImapUsernameFromMacro(imap);
            }

            var smtp = provider.outgoingServer.FirstOrDefault(x => x.type.ContainsIgnoreCase("smtp"));
            if (smtp == null)
                return;

            UseImapCredentialsForSmtp = false;
            SmtpHost = smtp.hostname;
            SmtpSecurity = smtp.socketType.ToSecurityPolicy();
            SmtpPort = short.Parse(smtp.port);
            SmtpUsername = GetSmtpUsernameFromMacro(smtp);
        }

        private string GetImapUsernameFromMacro(clientConfigEmailProviderIncomingServer config) {
            return config.username == "%EMAILADDRESS%" ? Address : Address.Split('@').First();
        }

        private string GetSmtpUsernameFromMacro(clientConfigEmailProviderOutgoingServer config) {
            return config.username == "%EMAILADDRESS%" ? Address : Address.Split('@').First();
        }

        internal async Task DeleteAsync() {
            try {
                foreach (var mailbox in Mailboxes) {
                    await mailbox.DeleteLocalAsync();
                }

                using (var database = new DatabaseContext()) {
                    var contactModels = await database.MailContacts
                        .ToArrayAsync();

                    database.MailContacts.RemoveRange(contactModels);
                    database.MailAccounts.Attach(_account);
                    database.MailAccounts.Remove(_account);

                    await database.SaveChangesAsync();
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        internal MailboxContext GetInbox() {
            return _mailboxes.FirstOrDefault(x => x.IsInbox);
        }

        internal async Task RestoreMessagesAsync(ICollection<MailMessageContext> messages) {
            try {
                var inbox = GetInbox();
                var trash = GetTrashMailbox();
                using (var connection = new ImapConnection { Security = _account.ImapSecurity }) {
                    using (var auth = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                        using (var session = await auth.LoginAsync(_account.ImapUsername, _account.ImapPassword)) {
                            var mailbox = await session.SelectAsync(trash.Name);
                            await mailbox.MoveMailsAsync(messages.Select(x => x.Uid).ToArray(), inbox.Name);
                        }
                    }
                }

                using (var database = new DatabaseContext()) {
                    foreach (var message in messages) {
                        try {
                            var model = new MailMessageModel {
                                Id = message.Id,
                                MailboxId = Id
                            };

                            database.MailMessages.Attach(model);
                            database.MailMessages.Remove(model);
                        } catch (Exception ex) {
                            Logger.Error(ex);
                            throw;
                        }
                    }
                    await database.SaveChangesAsync();
                }

                App.Context.NotifyMessagesRemoved(messages);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        internal MailboxContext GetTrashMailbox() {
            Application.Current.AssertUIThread();

            return _mailboxes.FirstOrDefault(x => x.IsTrash
                || x.Name.EqualsIgnoreCase(TrashMailboxName));
        }

        internal MailboxContext GetSentMailbox() {
            Application.Current.AssertUIThread();

            return _mailboxes.FirstOrDefault(x => x.IsSent
                || x.Name.EqualsIgnoreCase(SentMailboxName));
        }

        internal MailboxContext GetJunkMailbox() {
            Application.Current.AssertUIThread();

            return _mailboxes.FirstOrDefault(x => x.IsJunk
                || x.Name.EqualsIgnoreCase(JunkMailboxName));
        }

        internal MailboxContext GetDraftMailbox() {
            Application.Current.AssertUIThread();

            return _mailboxes.FirstOrDefault(x => x.IsDraft
                || x.Name.EqualsIgnoreCase(DraftMailboxName));
        }

        internal void NotifyMailboxAdded(MailboxContext child) {
            _mailboxes.Add(child);
        }

        internal async Task<bool> TryDeleteAccountAsync() {
            using (var context = new DatabaseContext()) {
                await context.Database.Connection.OpenAsync();
                using (var transaction = context.Database.Connection.BeginTransaction()) {
                    try {

                        var mailboxes = await context.Mailboxes
                            .Where(x => x.AccountId == Id)
                            .ToArrayAsync();

                        foreach (var mailbox in mailboxes) {
                            var lMailbox = mailbox;
                            var messages = await context.MailMessages
                                .Where(x => x.MailboxId == lMailbox.Id)
                                .ToArrayAsync();

                            foreach (var message in messages) {
                                var lMessage = message;
                                var mimeMessages = await context.MimeMessages
                                    .Where(x => x.MessageId == lMessage.Id)
                                    .ToArrayAsync();

                                if (mimeMessages == null)
                                    continue;

                                foreach (var mimeMessage in mimeMessages) {
                                    context.MimeMessages.Remove(mimeMessage);
                                }
                                await context.SaveChangesAsync();
                            }

                            context.MailMessages.RemoveRange(messages);
                            await context.SaveChangesAsync();
                        }

                        context.Mailboxes.RemoveRange(mailboxes);
                        await context.SaveChangesAsync();

                        var acc = new MailAccountModel { Id = Id };
                        context.MailAccounts.Attach(acc);
                        context.MailAccounts.Remove(acc);

                        await context.SaveChangesAsync();

                        transaction.Commit();
                        return true;
                    } catch (Exception ex) {
                        Logger.Error(ex);

                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        internal void NotifyMailboxRemoved(MailboxContext mailbox) {
            Application.Current.AssertUIThread();
            _mailboxes.Remove(mailbox);
        }
    }
}
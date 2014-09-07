#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Net;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI.Commands;
using Newtonsoft.Json;
using MailMessage = System.Net.Mail.MailMessage;
using NLog;
using System.Windows;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class MailAccountContext : HierarchyContext {

        #region Private Fields

        private bool _isOnline;
        private bool _isTesting;
        private bool _isAutoDetectPreferred;
        private bool _isDetectingSettings;
        private TestingContext _testing;
        private bool _isManagingMailboxes;
        private bool _isMailboxSelectionAvailable;

        private readonly AppContext _appContext;
        private readonly MailAccountModel _account;
        private readonly ICommand _listMailboxesCommand;
        private readonly ICommand _selectMailboxCommand;
        private readonly ICommand _testSettingsCommand;
        private readonly ICommand _registerCommand;
        private readonly ICommand _showUnsubscribedMailboxesCommand;
        private readonly ICommand _hideUnsubscribedMailboxesCommand;
        private readonly OutboxContext _outbox;
        private readonly ObservableCollection<MailboxContext> _mailboxes;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        internal MailAccountContext(MailAccountModel account) {
            _account = account;
            _appContext = App.Context;
            _outbox = new OutboxContext(this);
            _registerCommand = new RelayCommand(OnRegister);
            _selectMailboxCommand = new RelayCommand(OnSelectMailbox);
            _listMailboxesCommand = new RelayCommand(OnListMailboxes);
            _testSettingsCommand = new RelayCommand(OnTestSettings);
            _showUnsubscribedMailboxesCommand = new RelayCommand(OnShowUnsubscribedMailboxes);
            _hideUnsubscribedMailboxesCommand = new RelayCommand(OnHideUnsubscribedMailboxes);
            _isAutoDetectPreferred = true;
            _mailboxes = new ObservableCollection<MailboxContext>();
            _mailboxes.CollectionChanged += (sender, e) => RaisePropertyChanged(() => Mailboxes);
            _mailboxes.CollectionChanged += (sender, e) => RaisePropertyChanged(() => Children);
        }

        private void OnHideUnsubscribedMailboxes(object obj) {
            IsManagingMailboxes = false;
            Mailboxes.ForEach(x => x.IsEditing = false);
        }

        private void OnShowUnsubscribedMailboxes(object obj) {
            IsManagingMailboxes = true;
            Mailboxes.ForEach(x => x.IsEditing = true);
        }

        #endregion

        private void OnListMailboxes(object obj) {
            IsMailboxSelectionAvailable = true;
        }

        private void OnSelectMailbox(object obj) {

        }

        private async void OnRegister(object obj) {
            await RegisterKeyWithServerAsync();
        }

        private async void OnTestSettings(object obj) {
            await TestSettingsAsync();
        }

        public ICommand RegisterCommand {
            get { return _registerCommand; }
        }

        public ICommand TestSettingsCommand {
            get { return _testSettingsCommand; }
        }

        public ICommand SelectMailboxCommand {
            get { return _selectMailboxCommand; }
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
            try {
                await SyncMailboxesAsync();

                var inbox = GetInbox();
                if (inbox != null) {
                    inbox.IdleAsync();
                }

                IsOnline = true;
            } catch (Exception ex) {
                IsOnline = false;
                Logger.Error(ex);
            }
        }

        internal void Clear() {
            _mailboxes.Clear();
        }

        internal async Task<List<ImapMailboxInfo>> ListMailboxesAsync(string pattern = "") {
            using (var connection = new ImapConnection { Security = ImapSecurity }) {
                connection.RemoteCertificateValidationFailed += (sender, e) => e.IsCanceled = false;
                using (var auth = await connection.ConnectAsync(ImapHost, ImapPort)) {
                    using (var session = await auth.LoginAsync(ImapUsername, ImapPassword)) {
                        var wildcard = string.IsNullOrEmpty(pattern) ? "%" : pattern;
                        return await session.ListAsync("", ImapMailbox.EncodeName(wildcard));
                    }
                }
            }
        }

        internal async Task<List<ImapMailboxInfo>> ListSubscribedMailboxesAsync(string pattern) {
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

        internal async Task SyncMailboxesAsync() {
            Application.Current.AssertBackgroundThread();

            try {
                App.Context.StatusText = Resources.SyncMailboxesStatus;

                var mailboxes = _mailboxes.ToArray();
                var remoteMailboxes = await ListMailboxesAsync();
                if (IsGmail) {
                    // Fetch gmail folders and assign automagically.
                    var gmail =
                        remoteMailboxes.FirstOrDefault(
                            x => x.Name.ContainsIgnoreCase("gmail") || x.Name.ContainsIgnoreCase("google mail"));
                    if (gmail != null) {
                        var pattern = string.Format("{0}{1}%", gmail.Name, gmail.Delimiter);
                        var localizedMailboxes = await ListMailboxesAsync(pattern);
                        remoteMailboxes.AddRange(localizedMailboxes);
                    }
                }

                var types = new List<MailboxType> { MailboxType.Inbox, MailboxType.Draft, MailboxType.Sent, MailboxType.Trash };

                foreach (var mailbox in mailboxes) {
                    if (mailbox.Type == MailboxType.Inbox) {
                        types.Remove(MailboxType.Inbox);
                    }
                    if (mailbox.Type == MailboxType.Draft) {
                        types.Remove(MailboxType.Draft);
                    }
                    if (mailbox.Type == MailboxType.Sent) {
                        types.Remove(MailboxType.Sent);
                    }
                    if (mailbox.Type == MailboxType.Trash) {
                        types.Remove(MailboxType.Trash);
                    }
                }

                foreach (var mailbox in remoteMailboxes.Where(x => _mailboxes.All(y => string.Compare(x.Fullname, y.Name, StringComparison.InvariantCultureIgnoreCase) != 0))) {
                    var context = new MailboxContext(this, new MailboxModel {
                        AccountId = _account.Id
                    });

                    await context.InsertAsync();
                    context.SubscribeToMostProbableType(types, mailbox);
                    await context.BindMailboxAsync(mailbox);

                    Application.Current.Dispatcher
                        .InvokeAsync(() => _mailboxes.Add(context));
                }

                var inbox = _mailboxes
                    .FirstOrDefault(x => x.IsInbox);

                if (inbox != null) {
                    inbox.IsSelected = true;
                }
            } finally {
                App.Context.ResetStatusText();
            }
        }

        internal async Task UpdateAsync() {
            using (var database = new DatabaseContext()) {
                database.MailAccounts.Attach(_account);
                database.Entry(_account).State = EntityState.Modified;
                await database.SaveChangesAsync();
            }
        }

        internal async Task LoadMailboxesAsync() {
            MailboxModel[] mailboxes;
            using (var context = new DatabaseContext()) {
                mailboxes = await context.Mailboxes
                    .Where(x => x.AccountId == _account.Id)
                    .ToArrayAsync();
            }

            _mailboxes.AddRange(mailboxes
                .Select(x => new MailboxContext(this, x)));

            var tasks = _mailboxes.Select(x => x.CountNotSeenAsync());
            await Task.WhenAll(tasks);

            var inbox = _mailboxes.FirstOrDefault(x => x.Type == MailboxType.Inbox);
            if (inbox != null) {
                inbox.IsSelected = true;
            }
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

        public bool IsOnline {
            get { return _isOnline; }
            set {
                if (_isOnline == value) {
                    return;
                }

                _isOnline = value;
                RaisePropertyChanged(() => IsOnline);
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

        //public IEnumerable<MailboxContext> DockedMailboxes {
        //    get {
        //        return _mailboxes.Where(x => x.IsDocked || x.IsSelected).ToArray();
        //    }
        //}

        public IEnumerable<MailboxContext> Mailboxes {
            get { return _mailboxes; }
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
                    await mailbox.DeleteAsync();
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

        internal async Task RegisterKeyWithServerAsync() {
            try {
                var info = AppContext.GetKeyDirectory();
                var key = File.ReadAllText(Path.Combine(info.FullName, Settings.Default.PublicKeyFile));

                var pair = new AddressKeyPair {
                    Address = Address,
                    Key = key
                };

                var json = JsonConvert.SerializeObject(pair);
                var bytes = Encoding.UTF8.GetBytes(json);

                using (var client = new WebClient()) {
                    client.Headers.Add(HttpRequestHeader.UserAgent, Settings.Default.UserAgent);
                    var address = string.Format("{0}/keys", Settings.Default.KeyServer);
                    var uri = new Uri(address, UriKind.Absolute);
                    await client.UploadDataTaskAsync(uri, bytes);
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
                var trash = GetTrash();
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

        private MailboxContext GetTrash() {
            return _mailboxes.FirstOrDefault(x => x.IsTrash);
        }

        internal void NotifyMailboxAdded(MailboxContext child) {
            _mailboxes.Add(child);
        }
    }
}
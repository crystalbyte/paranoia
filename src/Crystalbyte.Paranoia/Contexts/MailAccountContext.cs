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

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class MailAccountContext : SelectionObject {
        private bool _isTesting;
        private bool _isAutoDetectPreferred;
        private bool _isDetectingSettings;
        private bool _isOutboxSelected;
        private TestingContext _testing;
        private MailboxContext _selectedMailbox;
        private readonly AppContext _appContext;
        private readonly MailAccountModel _account;
        private readonly ICommand _dropMailboxCommand;
        private readonly ICommand _testSettingsCommand;
        private readonly ICommand _registerAccount;
        private readonly ICommand _restoreMessagesCommand; 
        private readonly OutboxContext _outbox;
        private readonly ObservableCollection<MailboxContext> _mailboxes;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        internal MailAccountContext(MailAccountModel account) {
            _account = account;
            _appContext = App.Context;
            _outbox = new OutboxContext(this);
            _registerAccount = new RelayCommand(OnRegister);
            _dropMailboxCommand = new DropAssignmentCommand(this);
            _restoreMessagesCommand  = new RestoreMessageCommand(this);
            _testSettingsCommand = new RelayCommand(OnTestSettings);
            _isAutoDetectPreferred = true;
            _mailboxes = new ObservableCollection<MailboxContext>();
        }

        private async void OnRegister(object obj) {
            await RegisterKeyWithServerAsync();
        }

        private async void OnTestSettings(object obj) {
            await TestSettingsAsync();
        }

        public ICommand RegisterCommand {
            get { return _registerAccount; }
        }

        public ICommand DropMailboxCommand {
            get { return _dropMailboxCommand; }
        }

        public ICommand RestoreMessagesCommand {
            get { return _restoreMessagesCommand; }
        }

        public ICommand TestSettingsCommand {
            get { return _testSettingsCommand; }
        }

        protected override async void OnSelectionChanged() {
            base.OnSelectionChanged();

            Clear();
            if (!IsSelected)
                return;

            await UpdateAsync();
        }

        public async Task UpdateAsync() {
            await LoadMailboxesFromDatabaseAsync();
            await SyncMailboxesAsync();
        }

        internal void Clear() {
            _mailboxes.Clear();
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

        internal async Task SyncMailboxesAsync() {
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

            var t1 = mailboxes
                .Where(x => !x.IsAssigned)
                .Select(x => x.AssignMostProbableAsync(remoteMailboxes));

            await Task.WhenAll(t1);

            var t2 = mailboxes
                .Where(x => x.IsAssigned)
                .Select(x => x.SyncMessagesAsync());

            await Task.WhenAll(t2);

            App.Context.ResetStatusText();
        }

        internal async Task SyncWithDatabaseAsync() {
            using (var database = new DatabaseContext()) {
                database.MailAccounts.Attach(_account);
                database.Entry(_account).State = EntityState.Modified;
                await database.SaveChangesAsync();
            }
        }

        internal async Task LoadMailboxesFromDatabaseAsync() {
            var mailboxes = await Task.Factory.StartNew(() => {
                using (var context = new DatabaseContext()) {
                    context.MailAccounts.Attach(_account);
                    return _account.Mailboxes.ToArray();
                }
            });
            _mailboxes.AddRange(mailboxes.Select(x => new MailboxContext(this, x)));
            var inbox = _mailboxes.FirstOrDefault(x => x.Type == MailboxType.Inbox);
            if (inbox != null) {
                inbox.IsSelected = true;
            }

            var tasks = _mailboxes.Select(x => x.CountNotSeenAsync());
            await Task.WhenAll(tasks);
        }


        public event EventHandler MailboxSelectionChanged;

        private async void OnMailboxSelectionChanged() {
            var handler = MailboxSelectionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);

            var mailbox = SelectedMailbox;
            if (mailbox == null) {
                return;
            }

            IsOutboxSelected = false;

            mailbox.IsAssignable = !mailbox.IsAssigned;
            if (mailbox.IsAssignable) {
                await SelectedMailbox.PrepareManualAssignmentAsync();
            }

            await App.Context.RefreshMessagesAsync();
            await mailbox.SyncMessagesAsync();

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

        public OutboxContext Outbox {
            get { return _outbox; }
        }

        public bool IsOutboxSelected {
            get { return _isOutboxSelected; }
            set {
                if (_isOutboxSelected == value) {
                    return;
                }
                _isOutboxSelected = value;
                RaisePropertyChanged(() => IsOutboxSelected);
                OnOutboxSelectionChanged();
            }
        }

        private async void OnOutboxSelectionChanged() {
            if (IsOutboxSelected) {
                Mailboxes.ForEach(x => x.IsSelected = false);
                await Outbox.LoadSmtpRequestsFromDatabaseAsync();
            } else {
                Outbox.Clear();
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

        public IEnumerable<MailboxContext> Mailboxes {
            get { return _mailboxes; }
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
                        Subject = message.Subject,
                        Mime = await message.ToMimeAsync()
                    };

                    account.SmtpRequests.Add(request);
                }

                await database.SaveChangesAsync();
            }
        }

        public async Task SaveAsync() {
            try {
                AddSystemMailboxes();
                using (var database = new DatabaseContext()) {
                    database.MailAccounts.Add(_account);
                    await database.SaveChangesAsync();
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void AddSystemMailboxes() {
            _account.Mailboxes.Add(new MailboxModel {
                Type = MailboxType.Inbox
            });
            _account.Mailboxes.Add(new MailboxModel {
                Type = MailboxType.Trash
            });
            _account.Mailboxes.Add(new MailboxModel {
                Type = MailboxType.Sent
            });
            _account.Mailboxes.Add(new MailboxModel {
                Type = MailboxType.Draft
            });
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

        public async Task DeleteAsync() {
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

        internal async Task LoadMessagesForContactAsync(MailContactContext contact) {
            var mailbox = SelectedMailbox;
            if (mailbox == null) {
                return;
            }

            await mailbox.LoadMessagesForContactAsync(contact);
            foreach (var box in Mailboxes.AsParallel()) {
                await box.CountNotSeenAsync();
            }
        }

        internal async Task LoadAllMessagesAsync() {
            var mailbox = SelectedMailbox;
            if (mailbox == null) {
                return;
            }

            await mailbox.LoadMessagesAsync();
            foreach (var box in Mailboxes.AsParallel()) {
                await box.CountNotSeenAsync();
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
    }
}
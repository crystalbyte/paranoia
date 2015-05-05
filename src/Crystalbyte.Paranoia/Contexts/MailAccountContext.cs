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
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using ARSoft.Tools.Net.Dns;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI.Commands;
using NLog;
using MailMessage = System.Net.Mail.MailMessage;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class MailAccountContext : HierarchyContext {
        #region Private Fields

        private bool _isTesting;
        private bool _isAutoDetectPreferred;
        private bool _isDetectingSettings;
        private bool _isSyncingMailboxes;
        private bool _isManagingMailboxes;
        private bool _isMailboxSelectionAvailable;
        private TestingContext _testing;

        private readonly AppContext _appContext;
        private readonly MailAccount _account;
        private readonly ICommand _listMailboxesCommand;
        private readonly ICommand _testSettingsCommand;
        private readonly ICommand _deleteAccountCommand;
        private readonly ICommand _configAccountCommand;
        private readonly ICommand _showUnsubscribedMailboxesCommand;
        private readonly ICommand _hideUnsubscribedMailboxesCommand;
        private readonly OutboxContext _outbox;
        private readonly ObservableCollection<MailboxContext> _mailboxes;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        internal MailAccountContext(MailAccount account) {
            _account = account;
            _appContext = App.Context;
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
                    Resources.DeleteAccountQuestion, Resources.ApplicationName, MessageBoxButton.YesNo) ==
                    MessageBoxResult.No) {
                    return;
                }
                App.Context.NotifyAccountDeleted(this);
                await DeleteAccountAsync();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private async Task DeleteAccountAsync() {
            Application.Current.AssertUIThread();

            try {
                var success = await Task.Run(() => TryDeleteAccountAsync());
                if (!success) {
                    await App.Context.PublishAccountAsync(this);
                }
            } catch (Exception ex) {
                Logger.Error(ex);
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

        public DateTime IsDefaultTime {
            get { return _account.IsDefaultTime; }
            set {
                if (_account.IsDefaultTime == value) {
                    return;
                }
                _account.IsDefaultTime = value;
                RaisePropertyChanged(() => IsDefaultTime);
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
            Application.Current.AssertUIThread();

            try {
                if (_mailboxes.Count == 0) {
                    await Task.Run(() => LoadMailboxesAsync());
                }

                if (!IsSyncingMailboxes) {
                    await SyncMailboxesAsync();
                }

                var inbox = GetInbox();
                if (inbox != null && !inbox.IsIdling) {
                    inbox.IdleAsync();
                }
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
                        var context = new MailboxContext(this, new Mailbox {
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
                        await
                            Application.Current.Dispatcher.InvokeAsync(
                                () => _mailboxes.Add(context));
                    }

                    await SaveAsync();

                    await Application.Current.Dispatcher.InvokeAsync(() => {
                        var inbox =
                            _mailboxes
                                .FirstOrDefault(
                                    x => x.IsInbox);
                        if (inbox != null &&
                            !IsExpanded) {
                            inbox.IsSelected =
                                true;
                        }
                    });
                } catch (Exception ex) {
                    Logger.Error(ex);
                }
            });

            IsSyncingMailboxes = false;
        }

        internal async Task SaveAsync() {
            Application.Current.AssertBackgroundThread();

            try {
                using (var database = new DatabaseContext()) {
                    if (_account.Id == 0) {
                        database.MailAccounts.Add(_account);
                    } else {
                        database.MailAccounts.Attach(_account);
                        database.Entry(_account).State = EntityState.Modified;
                    }

                    // Handle Optimistic Concurrency.
                    // https://msdn.microsoft.com/en-us/data/jj592904.aspx?f=255&MSPPError=-2147217396
                    while (true) {
                        try {
                            await database.SaveChangesAsync();
                            break;
                        } catch (DbUpdateConcurrencyException ex) {
                            ex.Entries.ForEach(x => x.Reload());
                            Logger.Info(ex);
                        }
                    }
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        internal async Task LoadMailboxesAsync() {
            Application.Current.AssertBackgroundThread();

            Mailbox[] models;
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
            Application.Current.AssertUIThread();

            IsTesting = true;

            TestConnectivity();
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

        private Task TestSmtpSettingsAsync() {
            Testing = new TestingContext {
                Message = Resources.TestingSmtpStatus
            };


            return Task.Run(async () => {
                try {
                    using (var connection = new SmtpConnection { Security = SmtpSecurity }) {
                        using (var auth = await connection.ConnectAsync(SmtpHost, SmtpPort)) {
                            var username = UseImapCredentialsForSmtp ? ImapUsername : SmtpUsername;
                            var password = UseImapCredentialsForSmtp ? ImapPassword : SmtpPassword;
                            await auth.LoginAsync(username, password);
                        }
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);

                    Application.Current.Dispatcher.Invoke(() => {
                        Testing = new TestingContext {
                            IsFaulted = true,
                            Message = ex.Message,
                        };
                    });
                }
            });
        }

        private Task TestImapSettingsAsync() {
            Testing = new TestingContext {
                Message = Resources.TestingImapStatus
            };

            return Task.Run(async () => {
                try {
                    using (var connection = new ImapConnection { Security = ImapSecurity }) {
                        using (var auth = await connection.ConnectAsync(ImapHost, ImapPort)) {
                            await auth.LoginAsync(ImapUsername, ImapPassword);
                        }
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);

                    Application.Current.Dispatcher.Invoke(() => {
                        Testing = new TestingContext {
                            IsFaulted = true,
                            Message = ex.Message,
                        };
                    });
                }
            });
        }

        private void TestConnectivity() {
            Testing = new TestingContext {
                Message = Resources.TestingConnectivityStatus
            };

            var available = NetworkInterface.GetIsNetworkAvailable();
            if (!available) {
                Testing = new TestingContext {
                    Message = Resources.TestingConnectivityStatus,
                    IsFaulted = true
                };
            }
        }

        internal async Task SaveCompositionsAsync(IEnumerable<MailMessage> messages) {
            await Task.Run(async () => {
                using (var database = new DatabaseContext()) {
                    var account = await database.MailAccounts.FindAsync(_account.Id);

                    foreach (var message in messages) {
                        var request = new MailComposition {
                            Date = DateTime.Now,
                            ToName = message.To.First().DisplayName,
                            ToAddress = message.To.First().Address,
                            Subject = message.Subject
                        };
                        var mime = await message.ToMimeAsync();
                        request.Mime = Encoding.UTF8.GetBytes(mime);

                        account.Compositions.Add(request);
                    }

                    // Handle Optimistic Concurrency.
                    // https://msdn.microsoft.com/en-us/data/jj592904.aspx?f=255&MSPPError=-2147217396
                    while (true) {
                        try {
                            await database.SaveChangesAsync();
                            break;
                        } catch (DbUpdateConcurrencyException ex) {
                            ex.Entries.ForEach(x => x.Reload());
                            Logger.Info(ex);
                        }
                    }
                }
            });
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

            Logger.Info(Resources.QueryingMozillaDatabase);
            var config = await Task.Run(async () => {
                try {
                    using (var client = new WebClient()) {
                        IsDetectingSettings = true;
                        var stream =
                            await
                                client.OpenReadTaskAsync(new Uri(url, UriKind.Absolute));
                        var serializer = new XmlSerializer(typeof(clientConfig));
                        return serializer.Deserialize(stream) as clientConfig;
                    }
                } catch (WebException ex) {
                    Logger.Error(ex);
                    return null;
                }
            });

            if (config != null) {
                Configure(config);
                IsDetectingSettings = false;
                return;
            }

            Logger.Info(Resources.QueryingMxRecords);
            var response = await Task.Run(() => {
                try {
                    return DnsClient.Default.Resolve(domain, RecordType.Mx);
                } catch (Exception ex) {
                    Logger.Error(ex);
                    return null;
                }
            });

            if (response != null && response.AnswerRecords.Count > 0) {
                ApplyMxRecords(response);
            }

            MakeEducatedGuessForMissingFields();
            IsDetectingSettings = false;
        }

        private void ApplyMxRecords(DnsMessage response) {
            foreach (var record in response.AnswerRecords) {
                var mx = record.ToString().Split(' ').ElementAtOrDefault(5);
                if (string.IsNullOrEmpty(mx)) {
                    continue;
                }

                if (mx.Contains("imap")) {
                    ImapHost = mx;
                    continue;
                }

                if (mx.Contains("smtp")) {
                    SmtpHost = mx;
                    continue;
                }

                ImapHost = mx;
                SmtpHost = mx;
                break;
            }
        }

        private void MakeEducatedGuessForMissingFields() {
            if (string.IsNullOrEmpty(ImapUsername)) {
                ImapUsername = Address ?? string.Empty;
            }

            if (string.IsNullOrEmpty(SmtpUsername)) {
                SmtpUsername = Address ?? string.Empty;
            }

            if (ImapSecurity == SecurityProtocol.None) {
                ImapSecurity = SecurityProtocol.Implicit;
            }

            if (SmtpSecurity == SecurityProtocol.None) {
                SmtpSecurity = SecurityProtocol.Implicit;
            }
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

                    // Handle Optimistic Concurrency.
                    // https://msdn.microsoft.com/en-us/data/jj592904.aspx?f=255&MSPPError=-2147217396
                    while (true) {
                        try {
                            await database.SaveChangesAsync();
                            break;
                        } catch (DbUpdateConcurrencyException ex) {
                            ex.Entries.ForEach(x => x.Reload());
                            Logger.Info(ex);
                        }
                    }
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
                            var model = new Data.MailMessage {
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

                    // Handle Optimistic Concurrency.
                    // https://msdn.microsoft.com/en-us/data/jj592904.aspx?f=255&MSPPError=-2147217396
                    while (true) {
                        try {
                            await database.SaveChangesAsync();
                            break;
                        } catch (DbUpdateConcurrencyException ex) {
                            ex.Entries.ForEach(x => x.Reload());
                            Logger.Info(ex);
                        }
                    }
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
            Application.Current.AssertBackgroundThread();

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
                                var mimeMessages = await context.MailData
                                    .Where(x => x.MessageId == lMessage.Id)
                                    .ToArrayAsync();

                                if (mimeMessages == null)
                                    continue;

                                foreach (var mimeMessage in mimeMessages) {
                                    context.MailData.Remove(mimeMessage);
                                }
                            }

                            context.MailMessages.RemoveRange(messages);
                        }

                        context.Mailboxes.RemoveRange(mailboxes);

                        var acc = new MailAccount { Id = Id };
                        context.MailAccounts.Attach(acc);
                        context.MailAccounts.Remove(acc);

                        // Handle Optimistic Concurrency.
                        // https://msdn.microsoft.com/en-us/data/jj592904.aspx?f=255&MSPPError=-2147217396
                        while (true) {
                            try {
                                await context.SaveChangesAsync();
                                break;
                            } catch (DbUpdateConcurrencyException ex) {
                                ex.Entries.ForEach(x => x.Reload());
                                Logger.Info(ex);
                            }
                        }

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
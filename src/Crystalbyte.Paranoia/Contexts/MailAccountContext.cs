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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using ARSoft.Tools.Net.Dns;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Data.SQLite;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI.Commands;
using NLog;

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

        private readonly Dictionary<MailboxContext, bool> _subscriptionChanges;

        private readonly AppContext _appContext;
        private readonly MailAccount _account;
        private readonly ICommand _listMailboxesCommand;
        private readonly ICommand _testSettingsCommand;
        private readonly ICommand _deleteAccountCommand;
        private readonly ICommand _configAccountCommand;
        private readonly ICommand _showUnsubscribedMailboxesCommand;
        private readonly ICommand _acceptSubscriptionChangesCommand;
        private readonly ObservableCollection<MailboxContext> _mailboxes;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly object _saveMutex;
        private bool _isQuickViewDisabled;

        #endregion

        #region Construction

        internal MailAccountContext(MailAccount account) {
            _account = account;
            _appContext = App.Context;
            _listMailboxesCommand = new RelayCommand(OnListMailboxes);
            _testSettingsCommand = new RelayCommand(OnTestSettings);
            _configAccountCommand = new RelayCommand(OnConfigAccount);
            _deleteAccountCommand = new RelayCommand(OnDeleteAccount);
            _showUnsubscribedMailboxesCommand = new RelayCommand(OnShowUnsubscribedMailboxes);
            _acceptSubscriptionChangesCommand = new RelayCommand(OnAcceptSubscriptionChanges);
            _isAutoDetectPreferred = true;
            _saveMutex = new object();

            _subscriptionChanges = new Dictionary<MailboxContext, bool>();

            _mailboxes = new ObservableCollection<MailboxContext>();
            _mailboxes.CollectionChanged += (sender, e) => {
                RaisePropertyChanged(() => HasMailboxes);
                RaisePropertyChanged(() => Children);
                RaisePropertyChanged(() => MailboxRoots);
            };

            IsExpandedChanged += OnIsExpandedChanged;
        }

        private async void OnDeleteAccount(object obj) {
            try {
                if (MessageBox.Show(Application.Current.MainWindow,
                    Resources.DeleteAccountQuestion, Resources.ApplicationName, MessageBoxButton.YesNo) ==
                    MessageBoxResult.No) {
                    return;
                }

                await DeleteAsync();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private async void OnIsExpandedChanged(object sender, EventArgs e) {
            try {
                if (!IsExpanded) {
                    return;
                }

                var tasks = Children
                    .OfType<MailboxContext>()
                    .Where(x => !x.NotSeenCount.HasValue)
                    .Select(x => x.CountMessagesAsync());

                await Task.WhenAll(tasks);

                var inbox = GetInbox();
                if (inbox != null) {
                    inbox.IsSelected = true;
                }

            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        public ICommand DeleteAccountCommand {
            get { return _deleteAccountCommand; }
        }


        private static void OnConfigAccount(object obj) {
            try {
                App.Context.ConfigureAccount((MailAccountContext)obj);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private async void OnAcceptSubscriptionChanges(object obj) {
            try {
                IsManagingMailboxes = false;
                Mailboxes.ForEach(x => x.IsEditing = false);

                await ApplySubscriptionsAsync();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
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

        public bool IsQuickViewDisabled {
            get { return _isQuickViewDisabled; }
            set {
                if (_isQuickViewDisabled == value) {
                    return;
                }
                _isQuickViewDisabled = value;
                RaisePropertyChanged(() => IsQuickViewDisabled);
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

        public ICommand AcceptSubscriptionChangesCommand {
            get { return _acceptSubscriptionChangesCommand; }
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
                    await LoadMailboxesAsync();
                }

                if (!IsSyncingMailboxes) {
                    await SyncMailboxesAsync();
                }

                var inbox = GetInbox();
                if (inbox != null && !inbox.IsIdling) {
                    await inbox.IdleAsync();
                }
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
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

        internal void ChangeSubscription(MailboxContext mailbox, bool subscribe) {
            if (_subscriptionChanges.ContainsKey(mailbox)) {
                _subscriptionChanges[mailbox] = subscribe;
            } else {
                _subscriptionChanges.Add(mailbox, subscribe);
            }
        }

        internal async Task ApplySubscriptionsAsync() {

            var toBeSubbed = _subscriptionChanges.Where(x => x.Value).Select(x => x.Key).ToArray();
            var toBeUnsubbed = _subscriptionChanges.Where(x => !x.Value).Select(x => x.Key).ToArray();

            if (toBeSubbed.Length == 0 && toBeUnsubbed.Length == 0) {
                return;
            }

            var applyRemote = Task.Run(async () => {
                using (var connection = new ImapConnection { Security = _account.ImapSecurity }) {
                    using (var auth = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                        using (var session = await auth.LoginAsync(_account.ImapUsername, _account.ImapPassword)) {
                            foreach (var mailbox in toBeSubbed) {
                                await session.SubscribeAsync(mailbox.Name);
                            }
                            foreach (var mailbox in toBeUnsubbed) {
                                await session.UnsubscribeAsync(mailbox.Name);
                            }
                        }
                    }
                }
            });

            var applyLocal = Task.Run(async () => {
                using (var context = new DatabaseContext()) {
                    var set = context.Set<Mailbox>();

                    foreach (var e in toBeSubbed.Select(mailbox => new Mailbox { Id = mailbox.Id })) {
                        set.Attach(e);
                        e.IsSubscribed = true;
                    }
                    foreach (var e in toBeUnsubbed.Select(mailbox => new Mailbox { Id = mailbox.Id })) {
                        set.Attach(e);
                        e.IsSubscribed = false;
                    }

                    await context.SaveChangesAsync(OptimisticConcurrencyStrategy.ClientWins);
                }
            });

            await Task.WhenAll(new[] { applyRemote, applyLocal });
        }

        internal async Task SyncMailboxesAsync() {
            Logger.Enter();

            Application.Current.AssertUIThread();

            if (IsSyncingMailboxes) {
                throw new InvalidOperationException();
            }

            try {
                IsSyncingMailboxes = true;

                var id = _account.Id;
                var localMailboxesTask = Task.Run(() => {
                    using (var context = new DatabaseContext()) {
                        return context.Mailboxes
                            .Where(x => x.AccountId == id)
                            .ToArrayAsync();
                    }
                });

                var syncResult = await Task.Run(async () => {
                    var listMailboxes = await ListMailboxesAsync();
                    var subscribedMailboxesTask = ListSubscribedMailboxesAsync();

                    // Fetch gmail folders and assign automagically.
                    var gmail = listMailboxes.FirstOrDefault(x =>
                            x.Name.ContainsIgnoreCase("gmail") ||
                            x.Name.ContainsIgnoreCase("google mail"));

                    if (gmail != null) {
                        var pattern = string.Format("{0}{1}%", gmail.Name,
                            gmail.Delimiter);
                        var localizedMailboxes = await ListMailboxesAsync(pattern);
                        listMailboxes.AddRange(localizedMailboxes);
                    }
                    var localMailboxes = await localMailboxesTask;
                    var addedMailboxes = listMailboxes.Where(x => localMailboxes
                        .All(y => string.Compare(x.Fullname, y.Name, StringComparison.InvariantCultureIgnoreCase) != 0))
                        .ToArray();

                    var removedMailboxes = localMailboxes.Where(x => listMailboxes
                        .All(y => string.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase) != 0))
                        .ToArray();

                    return new {
                        AddedMailboxes = addedMailboxes.ToDictionary(x => x.Name),
                        RemovedMailboxes = removedMailboxes.ToDictionary(x => x.Name),
                        SubscribedMailboxes = (await subscribedMailboxesTask).ToDictionary(x => x.Name)
                    };
                });

                // Insert added mailboxes.
                var models = await Task.Run(async () => {
                    var resultSet = new List<Mailbox>();
                    using (var context = new DatabaseContext()) {
                        // Prepare models for insertion.
                        foreach (var info in syncResult.AddedMailboxes.Select(x => x.Value)) {
                            var model = new Mailbox {
                                AccountId = id,
                                IsSubscribed = info.IsGmailAll
                                               || info.IsGmailDraft
                                               || info.IsGmailImportant
                                               || info.IsGmailJunk
                                               || info.IsGmailSent
                                               || info.IsGmailTrash
                                               || info.IsInbox
                                               || syncResult.SubscribedMailboxes.ContainsKey(info.Name),
                                Name = info.Fullname,
                                Delimiter = info.Delimiter.ToString(CultureInfo.InvariantCulture),
                            };

                            model.Flags.AddRange(info.Flags.Select(x => new MailboxFlag {
                                Value = x
                            }));

                            context.Mailboxes.Add(model);
                            resultSet.Add(model);
                        }

                        await context.SaveChangesAsync(OptimisticConcurrencyStrategy.ClientWins);
                        return resultSet;
                    }
                });

                var contexts = models.Select(x => new MailboxContext(this, x));
                _mailboxes.AddRange(contexts);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                IsSyncingMailboxes = false;
                Logger.Exit();
            }
        }

        internal async Task LoadMailboxesAsync() {
            Logger.Enter();

            Application.Current.AssertUIThread();

            try {
                var mailboxes = await Task.Run(async () => {
                    using (var context = new DatabaseContext()) {
                        return await context.Mailboxes
                            .Where(x => x.AccountId == _account.Id)
                            .Include(x => x.Flags)
                            .ToArrayAsync();
                    }
                });

                var contexts = mailboxes.Select(x => new MailboxContext(this, x)).ToArray();
                _mailboxes.AddRange(contexts);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                Logger.Exit();
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
                return _mailboxes
                    .Where(x => !string.IsNullOrEmpty(x.Name) && !x.Name.Contains(x.Delimiter))
                    .ToArray();
            }
        }

        public IEnumerable<object> Children {
            get {
                return _mailboxes
                    .Where(x => !string.IsNullOrEmpty(x.Name) && !x.Name.Contains(x.Delimiter))
                    .Cast<object>()
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

        private async Task TestSmtpSettingsAsync() {
            Testing = new TestingContext {
                Message = Resources.TestingSmtpStatus
            };

            try {
                await Task.Run(async () => {
                    using (var connection = new SmtpConnection { Security = SmtpSecurity }) {
                        using (var auth = await connection.ConnectAsync(SmtpHost, SmtpPort)) {
                            var username = UseImapCredentialsForSmtp ? ImapUsername : SmtpUsername;
                            var password = UseImapCredentialsForSmtp ? ImapPassword : SmtpPassword;
                            await auth.LoginAsync(username, password);
                        }
                    }
                });
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);

                Testing = new TestingContext {
                    IsFaulted = true,
                    Message = ex.Message,
                };
            }
        }

        private async Task TestImapSettingsAsync() {
            Testing = new TestingContext {
                Message = Resources.TestingImapStatus
            };

            try {
                await Task.Run(async () => {
                    using (var connection = new ImapConnection { Security = ImapSecurity }) {
                        using (var auth = await connection.ConnectAsync(ImapHost, ImapPort)) {
                            await auth.LoginAsync(ImapUsername, ImapPassword);
                        }
                    }
                });
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);

                Testing = new TestingContext {
                    IsFaulted = true,
                    Message = ex.Message,
                };
            }
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

            throw new NotImplementedException();

            //await Task.Run(async () => {
            //    using (var context = new DatabaseContext()) {
            //        var account = await context.MailAccounts.FindAsync(_account.Id);

            //        foreach (var message in messages) {
            //            var request = new MailComposition {
            //                Date = DateTime.Now,
            //                ToName = message.To.First().DisplayName,
            //                ToAddress = message.To.First().Address,
            //                Subject = message.Subject
            //            };
            //            var mime = await message.ToMimeAsync();
            //            request.Mime = Encoding.UTF8.GetBytes(mime);

            //            account.Compositions.Add(request);
            //        }

            //        await context.SaveChangesAsync(OptimisticConcurrencyStrategy.ClientWins);
            //    }
            //});
        }

        public async Task DetectSettingsAsync() {
            var domain = Address.Split('@').Last();
            var url = string.Format("https://liv" +
                                    "e.mozillamessaging.com/autoconfig/v1.1/{0}", domain);

            IsDetectingSettings = true;

            Logger.Info(Resources.QueryingMozillaDatabase);
            var config = await Task.Run(async () => {
                try {
                    using (var client = new WebClient()) {
                        var stream =
                            await
                                client.OpenReadTaskAsync(new Uri(url, UriKind.Absolute));
                        var serializer = new XmlSerializer(typeof(clientConfig));
                        return serializer.Deserialize(stream) as clientConfig;
                    }
                } catch (WebException ex) {
                    Logger.ErrorException(ex.Message, ex);
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
                    Logger.ErrorException(ex.Message, ex);
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
            Logger.Enter();

            Application.Current.AssertUIThread();

            App.Context.NotifyAccountRemoved(this);

            try {
                await Task.Run(async () => {
                    using (var context = new DatabaseContext()) {
                        await context.OpenAsync();
                        await context.EnableForeignKeysAsync();

                        var account = await context.Set<MailAccount>()
                            .SingleOrDefaultAsync(x => x.Id == Id);

                        var mailboxes = await context.Set<Mailbox>()
                            .Include(x => x.Flags)
                            .Where(x => x.AccountId == Id)
                            .ToDictionaryAsync(x => x.Id);

                        var mailboxIds = mailboxes.Select(x => x.Key).ToArray();
                        var messages = await context.Set<MailMessage>()
                            .Include(x => x.Flags)
                            .Include(x => x.Addresses)
                            .Include(x => x.Attachments)
                            .Where(x => mailboxIds.Contains(x.MailboxId))
                            .ToDictionaryAsync(x => x);

                        var tableAttribute = typeof(MailMessageContent).GetCustomAttribute<TableAttribute>();
                        var tableName = tableAttribute == null ? typeof(MailMessageContent).Name : tableAttribute.Name;

                        var keyProperty = typeof(MailMessageContent).GetProperties()
                            .Single(x => x.GetCustomAttribute<KeyAttribute>() != null);

                        var keyColumnAttribute = keyProperty.GetCustomAttribute<ColumnAttribute>();
                        var keyColumnName = keyColumnAttribute == null ? keyProperty.Name : keyColumnAttribute.Name;

                        // We need to drop the full text index manually, for it is not associated through foreign keys.
                        var ids = account.Mailboxes.SelectMany(x => x.Messages.Select(y => y.Id));
                        var joinedIds = string.Join(",", ids);

                        using (var transaction = context.Database.BeginTransaction()) {
                            try {
                                var command = string.Format("DELETE FROM {0} WHERE {1} IN ({2});", tableName, keyColumnName, joinedIds);
                                await context.Database.ExecuteSqlCommandAsync(command);

                                context.Set<MailMessage>().RemoveRange(messages.Values);
                                context.Set<Mailbox>().RemoveRange(mailboxes.Values);
                                context.Set<MailAccount>().Remove(account);

                                await context.SaveChangesAsync(
                                    OptimisticConcurrencyStrategy.DatabaseWins);

                                transaction.Commit();
                            } catch (Exception ex) {
                                transaction.Rollback();
                                Logger.ErrorException(ex.Message, ex);
                            }
                        }
                    }
                });


            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
                App.Context.NotifyAccountCreated(this);
            } finally {
                Logger.Exit();
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
                            var model = new MailMessage {
                                Id = message.Id,
                                MailboxId = Id
                            };

                            database.MailMessages.Attach(model);
                            database.MailMessages.Remove(model);
                        } catch (Exception ex) {
                            Logger.ErrorException(ex.Message, ex);
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

                // Requery message source for some items may have changed.
                await App.Context.QueryMessageSourceAsync();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
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

        internal void NotifyMailboxesAdded(IEnumerable<MailboxContext> contexts) {
            Application.Current.AssertUIThread();

            foreach (var context in contexts) {
                _mailboxes.Add(context);
            }
        }

        internal void NotifyMailboxesRemoved(IEnumerable<MailboxContext> contexts) {
            Application.Current.AssertUIThread();

            foreach (var context in contexts) {
                _mailboxes.Remove(context);
            }
        }

        /// <summary>
        /// Saves the underlying account model to the data store.
        /// </summary>
        /// <returns>Returns the task state object.</returns>
        public Task SaveAsync() {
            return Task.Run(() => {

                // We need to lock this call for the entity must not be attached multiple times.
                lock (_saveMutex) {
                    using (var context = new DatabaseContext()) {

                        if (_account.Id == 0) {
                            context.MailAccounts.Add(_account);
                        } else {
                            context.MailAccounts.Attach(_account);
                            context.Entry(_account).State = EntityState.Modified;
                        }

                        // Handle Optimistic Concurrency.
                        // https://msdn.microsoft.com/en-us/data/jj592904.aspx?f=255&MSPPError=-2147217396
                        while (true) {
                            try {
                                // Do not use await/async inside a lock.
                                context.SaveChanges();
                                break;
                            } catch (DbUpdateConcurrencyException ex) {
                                Logger.Warn(ex);
                                ex.Entries.ForEach(x => x.OriginalValues.SetValues(x.GetDatabaseValues()));
                            }
                        }
                    }
                }
            });
        }
    }
}
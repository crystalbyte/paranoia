#region Using directives

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Net;
using Crystalbyte.Paranoia.Properties;
using Newtonsoft.Json;
using NLog;
using System.Text.RegularExpressions;

#endregion

namespace Crystalbyte.Paranoia {
    [DebuggerDisplay("Name = {Name}")]
    public sealed class MailboxContext : HierarchyContext, IMessageSource {

        #region Private Fields

        private int _notSeenCount;
        private bool _isSyncingMessages;
        private bool _isDownloadingMessage;
        private bool _isLoadingMessages;
        private bool _isListingMailboxes;
        private readonly MailboxModel _mailbox;
        private readonly MailAccountContext _account;
        private bool _isLoadingChildren;
        private bool _isSyncingChildren;
        private int _totalEnvelopeCount;
        private int _fetchedEnvelopeCount;
        private bool _isEditing;
        private bool _isIdling;
        private bool _showAllMessages;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        internal MailboxContext(MailAccountContext account, MailboxModel mailbox) {
            _account = account;
            _mailbox = mailbox;
            _showAllMessages = true;
        }

        #endregion

        public MailAccountContext Account {
            get { return _account; }
        }

        public string Name {
            get { return _mailbox.Name; }
            set {
                if (_mailbox.Name == value) {
                    return;
                }

                _mailbox.Name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        public bool IsEditing {
            get { return _isEditing; }
            set {
                if (_isEditing == value) {
                    return;
                }

                _isEditing = value;
                RaisePropertyChanged(() => IsEditing);
                RaisePropertyChanged(() => IsListed);
            }
        }

        public bool HasListedChildren {
            get { return HasChildren && Children.Any(x => x.IsListed); }
        }

        public override bool IsListed {
            get { return IsEditing || IsSubscribed || HasListedChildren; }
        }

        public string LocalName {
            get {
                return string.IsNullOrEmpty(Name)
                    ? string.Empty
                    : Name.Split(new[] { Delimiter },
                    StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            }
        }

        public Int64 Id {
            get { return _mailbox.Id; }
            set {
                if (_mailbox.Id == value) {
                    return;
                }

                _mailbox.Id = value;
                RaisePropertyChanged(() => Id);
            }
        }

        protected override void OnSelectionChanged() {
            base.OnSelectionChanged();

            Application.Current.AssertUIThread();
            try {
                Task.Run(() => SyncChildrenAsync());
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private async Task SyncChildrenAsync() {
            Application.Current.AssertBackgroundThread();

            try {
                IsSyncingChildren = true;

                var pattern = string.Format("{0}{1}%", Name, Delimiter);
                var children = await _account.ListMailboxesAsync(pattern);
                var subscribed = await _account.ListSubscribedMailboxesAsync(pattern);

                using (var database = new DatabaseContext()) {
                    foreach (var child in children) {
                        var c = child;
                        var mailbox = await database.Mailboxes.FirstOrDefaultAsync(x => x.Name == c.Fullname);
                        if (mailbox != null) {
                            continue;
                        }

                        var context = new MailboxContext(_account, new MailboxModel {
                            AccountId = _account.Id
                        });

                        await context.InsertAsync();
                        await context.BindMailboxAsync(child, subscribed);

                        Application.Current.Dispatcher
                            .InvokeAsync(() => _account.NotifyMailboxAdded(context));
                    }
                }
            } finally {
                IsSyncingChildren = false;
                RaisePropertyChanged(() => Children);
            }
        }

        public bool IsLoadingChildren {
            get { return _isLoadingChildren; }
            set {
                if (_isLoadingChildren == value) {
                    return;
                }
                _isLoadingChildren = value;
                RaisePropertyChanged(() => IsLoadingChildren);
            }
        }

        public bool IsSyncingChildren {
            get { return _isSyncingChildren; }
            set {
                if (_isSyncingChildren == value) {
                    return;
                }
                _isSyncingChildren = value;
                RaisePropertyChanged(() => IsSyncingChildren);
            }
        }

        public IEnumerable<MailboxContext> Children {
            get {
                var prefix = string.Format("{0}{1}", Name, Delimiter);
                var mailboxes = _account.Mailboxes
                  .Where(x => x.Name.StartsWith(prefix)).ToArray();

                var m = mailboxes
                    .Where(x => !x.Name.Substring(prefix.Length, x.Name.Length - prefix.Length).Contains(x.Delimiter))
                    .ToArray();

                return m;
            }
        }

        internal string Delimiter {
            get { return _mailbox.Delimiter; }
        }

        internal async Task InsertAsync() {
            try {
                using (var database = new DatabaseContext()) {
                    database.Mailboxes.Add(_mailbox);
                    await database.SaveChangesAsync();
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        internal async Task UpdateAsync() {
            try {
                using (var database = new DatabaseContext()) {
                    database.Mailboxes.Attach(_mailbox);
                    database.Entry(_mailbox).State = EntityState.Modified;
                    await database.SaveChangesAsync();
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        internal async Task DeleteMessagesAsync(MailMessageContext[] messages, string trashFolder) {
            try {
                using (var connection = new ImapConnection { Security = _account.ImapSecurity }) {
                    using (var auth = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                        using (var session = await auth.LoginAsync(_account.ImapUsername, _account.ImapPassword)) {
                            var mailbox = await session.SelectAsync(Name);
                            if (Type == MailboxType.Trash) {
                                await mailbox.DeleteMailsAsync(messages.Select(x => x.Uid));
                            } else {
                                await mailbox.MoveMailsAsync(messages.Select(x => x.Uid).ToArray(), trashFolder);
                            }
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
                        }
                    }
                    await database.SaveChangesAsync();
                }

                App.Context.NotifyMessagesRemoved(messages);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        public bool IsSubscribed {
            get { return _mailbox.IsSubscribed; }
            set {
                if (_mailbox.IsSubscribed == value) {
                    return;
                }
                _mailbox.IsSubscribed = value;
                RaisePropertyChanged(() => IsSubscribed);
                RaisePropertyChanged(() => IsListed);

                if (value) {
                    Task.Run(() => SubscribeAsync());
                } else {
                    Task.Run(() => UnsubscribeAsync());
                }
            }
        }

        internal bool IsTrash {
            get { return Type == MailboxType.Trash; }
        }

        public bool IsInbox {
            get { return _mailbox.Type == MailboxType.Inbox; }
        }

        public bool IsListingMailboxes {
            get { return _isListingMailboxes; }
            set {
                if (_isListingMailboxes == value) {
                    return;
                }
                _isListingMailboxes = value;
                RaisePropertyChanged(() => IsListingMailboxes);
            }
        }

        public int NotSeenCount {
            get { return _notSeenCount; }
            set {
                if (_notSeenCount == value) {
                    return;
                }
                _notSeenCount = value;
                RaisePropertyChanged(() => NotSeenCount);
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
            }
        }

        public bool IsSyncingMessages {
            get { return _isSyncingMessages; }
            set {
                if (_isSyncingMessages == value) {
                    return;
                }
                _isSyncingMessages = value;
                RaisePropertyChanged(() => IsSyncingMessages);
            }
        }

        public bool IsDownloadingMessage {
            get { return _isDownloadingMessage; }
            set {
                if (_isDownloadingMessage == value) {
                    return;
                }
                _isDownloadingMessage = value;
                RaisePropertyChanged(() => IsDownloadingMessage);
            }
        }

        public bool IsSelectable {
            get { return !_mailbox.Flags.ContainsIgnoreCase(MailboxFlags.NoSelect); }
        }

        public bool IsSystemMailbox {
            get { return _mailbox.Type != MailboxType.Custom; }
        }

        private async Task SubscribeAsync() {
            try {
                using (var connection = new ImapConnection { Security = _account.ImapSecurity }) {
                    using (var auth = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                        using (var session = await auth.LoginAsync(_account.ImapUsername, _account.ImapPassword)) {
                            await session.SubscribeAsync(Name);
                        }
                    }
                }

                using (var database = new DatabaseContext()) {
                    var mailbox = await database.Mailboxes.FindAsync(_mailbox.Id);
                    mailbox.IsSubscribed = true;
                    await database.SaveChangesAsync();
                }

            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }
        private async Task UnsubscribeAsync() {
            try {
                using (var connection = new ImapConnection { Security = _account.ImapSecurity }) {
                    using (var auth = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                        using (var session = await auth.LoginAsync(_account.ImapUsername, _account.ImapPassword)) {
                            await session.UnsubscribeAsync(Name);
                        }
                    }
                }

                using (var database = new DatabaseContext()) {
                    var mailbox = await database.Mailboxes.FindAsync(_mailbox.Id);
                    mailbox.IsSubscribed = false;
                    await database.SaveChangesAsync();
                }

            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        public MailboxType Type {
            get { return _mailbox.Type; }
            set {
                if (_mailbox.Type == value) {
                    return;
                }

                _mailbox.Type = value;
                RaisePropertyChanged(() => Type);
            }
        }

        private Task<MailAccountModel> GetAccountAsync() {
            using (var context = new DatabaseContext()) {
                return context.MailAccounts.FindAsync(_mailbox.AccountId);
            }
        }

        internal async Task SyncMessagesAsync() {
            Application.Current.AssertBackgroundThread();

            try {
                if (IsSyncingMessages) {
                    return;
                }

                IsSyncingMessages = true;

                var name = _mailbox.Name;
                var maxUid = await GetMaxUidAsync();
                var account = await GetAccountAsync();

                var messages = new List<MailMessageModel>();

                using (var connection = new ImapConnection { Security = account.ImapSecurity }) {
                    connection.RemoteCertificateValidationFailed += (sender, e) => e.IsCanceled = false;
                    using (var auth = await connection.ConnectAsync(account.ImapHost, account.ImapPort)) {
                        using (var session = await auth.LoginAsync(account.ImapUsername, account.ImapPassword)) {
                            var mailbox = await session.SelectAsync(name);

                            messages.AddRange(await SyncChallengesAsync(mailbox, maxUid));
                            messages.AddRange(await SyncNonChallengesAsync(mailbox, maxUid));
                        }
                    }
                }

                if (messages.Count == 0) {
                    FetchedEnvelopeCount = 0;
                    return;
                }

                await SaveContactsAsync(messages);
                await SaveMessagesAsync(messages);

                var contexts = messages.Where(x => x.Type == MailType.Message)
                    .Select(x => new MailMessageContext(this, x)).ToArray();

                FetchedEnvelopeCount = 0;

                await Application.Current.Dispatcher.InvokeAsync(() => {
                    App.Context.NotifyMessagesAdded(contexts);
                    if (!IsInbox || contexts.Length <= 0)
                        return;

                    var notification = new NotificationWindow(contexts);
                    notification.Show();
                });
            } finally {
                IsSyncingMessages = false;
            }
        }

        public int TotalEnvelopeCount {
            get { return _totalEnvelopeCount; }
            set {
                if (_totalEnvelopeCount == value) {
                    return;
                }
                _totalEnvelopeCount = value;
                RaisePropertyChanged(() => TotalEnvelopeCount);
            }
        }

        public int FetchedEnvelopeCount {
            get { return _fetchedEnvelopeCount; }
            set {
                if (_fetchedEnvelopeCount == value) {
                    return;
                }
                _fetchedEnvelopeCount = value;
                RaisePropertyChanged(() => FetchedEnvelopeCount);
            }
        }

        private async Task<IEnumerable<MailMessageModel>> SyncNonChallengesAsync(ImapMailbox mailbox, long uid) {
            var criteria = string.Format("{0}:* NOT HEADER \"{1}\" \"{2}\"", uid, ParanoiaHeaderKeys.Type, MailType.Challenge);
            var uids = await mailbox.SearchAsync(criteria);
            if (!uids.Any()) {
                return new MailMessageModel[0];
            }

            FetchedEnvelopeCount = 0;
            TotalEnvelopeCount = uids.Count;

            mailbox.EnvelopeFetched += OnEnvelopeFetched;
            var envelopes = (await mailbox.FetchEnvelopesAsync(uids)).ToArray();
            if (envelopes.Length == 0) {
                return new MailMessageModel[0];
            }
            mailbox.EnvelopeFetched -= OnEnvelopeFetched;

            if (envelopes.Length == 1 && envelopes.First().Uid == uid) {
                return new MailMessageModel[0];
            }

            var messages = new List<MailMessageModel>();
            foreach (var envelope in envelopes) {
                try {
                    var message = envelope.ToMailMessage(MailType.Message);
                    messages.Add(message);
                } catch (Exception ex) {
                    Logger.Error(ex);
                }
            }
            return messages;
        }

        private void OnEnvelopeFetched(object sender, EnvelopeFetchedEventArgs e) {
            FetchedEnvelopeCount++;
        }

        private static async Task<IEnumerable<MailMessageModel>> SyncChallengesAsync(ImapMailbox mailbox, long uid) {
            var criteria = string.Format("{0}:* HEADER \"{1}\" \"{2}\"", uid, ParanoiaHeaderKeys.Type, MailType.Challenge);
            var uids = await mailbox.SearchAsync(criteria);
            if (!uids.Any()) {
                return new MailMessageModel[0];
            }

            var envelopes = (await mailbox.FetchEnvelopesAsync(uids)).ToArray();
            if (envelopes.Length == 0) {
                return new MailMessageModel[0];
            }

            if (envelopes.Length == 1 && envelopes.First().Uid == uid) {
                return new MailMessageModel[0];
            }

            var now = DateTime.Now;
            foreach (var envelope in envelopes.Where(x => x.InternalDate.HasValue
                && now.Subtract(x.InternalDate.Value) < TimeSpan.FromHours(1))) {
                try {
                    await ProcessChallengeAsync(envelope, mailbox);
                } catch (Exception ex) {
                    Logger.Error(ex);
                }
            }

            var messages = new List<MailMessageModel>();
            foreach (var envelope in envelopes) {
                try {
                    var message = envelope.ToMailMessage(MailType.Challenge);
                    messages.Add(message);
                } catch (Exception ex) {
                    Logger.Error(ex);
                }
            }
            return messages;
        }

        private static async Task ProcessChallengeAsync(ImapEnvelope envelope, ImapMailbox mailbox) {
            var body = await mailbox.FetchMessageBodyAsync(envelope.Uid);
            var bytes = Encoding.UTF8.GetBytes(body);
            var message = new MailMessageReader(bytes);

            var token = string.Empty;
            var nonce = string.Empty;
            var publicKey = string.Empty;

            const string pattern = @"\s|\t|\n|\r";

            var xHeaders = message.Headers.UnknownHeaders;
            for (var i = 0; i < xHeaders.Keys.Count; i++) {
                var key = xHeaders.Keys[i];

                var values = xHeaders.GetValues(i);
                if (values == null) {
                    throw new NullReferenceException(Resources.ChallengeCorruptException);
                }

                if (string.Compare(key, ParanoiaHeaderKeys.Token, StringComparison.InvariantCultureIgnoreCase) == 0) {
                    token = values.FirstOrDefault() ?? string.Empty;
                    token = Regex.Replace(token, pattern, string.Empty);
                    continue;
                }

                if (string.Compare(key, ParanoiaHeaderKeys.Nonce, StringComparison.InvariantCultureIgnoreCase) == 0) {
                    nonce = values.FirstOrDefault() ?? string.Empty;
                    nonce = Regex.Replace(nonce, pattern, string.Empty);
                    continue;
                }

                if (string.Compare(key, ParanoiaHeaderKeys.PublicKey, StringComparison.InvariantCultureIgnoreCase) == 0) {
                    publicKey = values.FirstOrDefault() ?? string.Empty;
                    publicKey = Regex.Replace(publicKey, pattern, string.Empty);
                }
            }

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(nonce) || string.IsNullOrEmpty(publicKey)) {
                throw new InvalidDataException(Resources.ChallengeCorruptException);
            }


            var data = App.Context.KeyContainer.DecryptWithPrivateKey(
                Convert.FromBase64String(token),
                Convert.FromBase64String(publicKey),
                Convert.FromBase64String(nonce));

            await RespondToChallengeAsync(Encoding.UTF8.GetString(data));
        }

        private static async Task RespondToChallengeAsync(string challenge) {
            var response = JsonConvert.SerializeObject(new ChallengeResponse {
                Token = challenge
            });

            using (var client = new WebClient()) {
                client.Headers.Add(HttpRequestHeader.UserAgent, Settings.Default.UserAgent);

                var address = string.Format("{0}/verify", Settings.Default.KeyServer);
                var stream = await client.OpenWriteTaskAsync(new Uri(address, UriKind.Absolute));

                using (var writer = new StreamWriter(stream)) {
                    await writer.WriteAsync(response);
                }
            }
        }

        private static async Task SaveContactsAsync(IEnumerable<MailMessageModel> messages) {
            try {
                var groups = messages.GroupBy(x => x.FromAddress).ToArray();

                using (var database = new DatabaseContext()) {
                    var contacts = await database.MailContacts
                        .GroupBy(x => x.Address)
                        .ToArrayAsync();

                    var diff = groups.Where(x => contacts
                        .All(y => string.Compare(x.Key, y.Key,
                            StringComparison.InvariantCultureIgnoreCase) != 0))
                        .ToArray();


                    var contexts = new List<MailContactContext>();
                    foreach (var model in diff.Select(group => new MailContactModel {
                        Address = group.First().FromAddress,
                        Name = group.First().FromName
                    })) {
                        database.MailContacts.Add(model);
                        contexts.Add(new MailContactContext(model));
                    }

                    if (diff.Length < 1) {
                        return;
                    }

                    await database.SaveChangesAsync();

                    await Application.Current.Dispatcher
                        .InvokeAsync(() => App.Context.NotifyContactsAdded(contexts));
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        internal async Task MarkAsNotSeenAsync(MailMessageContext[] messages) {
            Application.Current.AssertBackgroundThread();

            try {
                messages.ForEach(x => x.IsSeen = false);
                var uids = messages.Select(x => x.Uid).ToArray();

                using (var connection = new ImapConnection { Security = _account.ImapSecurity }) {
                    connection.RemoteCertificateValidationFailed += (sender, e) => e.IsCanceled = false;
                    using (var auth = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                        using (var session = await auth.LoginAsync(_account.ImapUsername, _account.ImapPassword)) {
                            var folder = await session.SelectAsync(Name);
                            await folder.MarkAsNotSeenAsync(uids);
                        }
                    }
                }

                await CountNotSeenAsync();

                var contact = App.Context.SelectedContact;
                if (contact != null) {
                    await contact.CountNotSeenAsync();
                }
            } catch (Exception ex) {
                messages.ForEach(x => x.IsSeen = true);
                Logger.Error(ex);
            }
        }

        internal async Task MarkAsSeenAsync(MailMessageContext[] messages) {
            Application.Current.AssertBackgroundThread();

            try {
                messages.ForEach(x => x.IsSeen = true);
                var uids = messages.Select(x => x.Uid).ToArray();

                using (var connection = new ImapConnection { Security = _account.ImapSecurity }) {
                    connection.RemoteCertificateValidationFailed += (sender, e) => e.IsCanceled = false;
                    using (var auth = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                        using (var session = await auth.LoginAsync(_account.ImapUsername, _account.ImapPassword)) {
                            var folder = await session.SelectAsync(Name);
                            await folder.MarkAsSeenAsync(uids);
                        }
                    }
                }

                await CountNotSeenAsync();

                var contact = App.Context.SelectedContact;
                if (contact != null) {
                    await contact.CountNotSeenAsync();
                }
            } catch (Exception ex) {
                messages.ForEach(x => x.IsSeen = false);
                Logger.Error(ex);
            }
        }

        internal async Task CountNotSeenAsync() {
            Application.Current.AssertBackgroundThread();

            using (var context = new DatabaseContext()) {
                NotSeenCount = await context.MailMessages
                    .Where(x => x.Type == MailType.Message)
                    .Where(x => x.MailboxId == _mailbox.Id)
                    .Where(x => !x.Flags.Contains(MailboxFlags.Seen))
                    .CountAsync();
            }
        }

        private async Task SaveMessagesAsync(IEnumerable<MailMessageModel> messages) {
            using (var context = new DatabaseContext()) {
                var mailbox = await context.Mailboxes.FindAsync(_mailbox.Id);
                mailbox.Messages.AddRange(messages);
                await context.SaveChangesAsync();
            }
        }

        private Task<Int64> GetMaxUidAsync() {
            using (var context = new DatabaseContext()) {
                return context.MailMessages
                    .Where(x => x.MailboxId == _mailbox.Id)
                    .Select(x => x.Uid)
                    .DefaultIfEmpty(1)
                    .MaxAsync(x => x);
            }
        }

        internal void SubscribeToMostProbableType(List<MailboxType> types, ImapMailboxInfo mailbox) {

            if (mailbox.IsInbox || string.Compare(Name, "inbox", StringComparison.InvariantCultureIgnoreCase) >= 0) {
                types.Remove(MailboxType.Inbox);
                _mailbox.Type = MailboxType.Inbox;
                IsSubscribed = true;
            }

            if (mailbox.IsGmailSent || string.Compare(Name, "sent", StringComparison.InvariantCultureIgnoreCase) >= 0) {
                types.Remove(MailboxType.Sent);
                _mailbox.Type = MailboxType.Sent;
                IsSubscribed = true;
            }

            if (mailbox.IsGmailDraft || string.Compare(Name, "draft", StringComparison.InvariantCultureIgnoreCase) >= 0) {
                types.Remove(MailboxType.Draft);
                _mailbox.Type = MailboxType.Draft;
                IsSubscribed = true;
            }

            if (mailbox.IsGmailTrash || string.Compare(Name, "trash", StringComparison.InvariantCultureIgnoreCase) >= 0) {
                types.Remove(MailboxType.Trash);
                _mailbox.Type = MailboxType.Trash;
                IsSubscribed = true;
            }
        }

        internal async Task BindMailboxAsync(ImapMailboxInfo mailbox) {
            await BindMailboxAsync(mailbox, new ImapMailboxInfo[0]);
        }

        private async Task BindMailboxAsync(ImapMailboxInfo mailbox, IEnumerable<ImapMailboxInfo> subscriptions) {
            try {
                // If no match has been found mailbox will be null.
                if (mailbox == null) {
                    return;
                }

                using (var context = new DatabaseContext()) {
                    context.Mailboxes.Attach(_mailbox);

                    _mailbox.Name = mailbox.Fullname;
                    _mailbox.Delimiter = mailbox.Delimiter.ToString(CultureInfo.InvariantCulture);
                    _mailbox.Flags = mailbox.Flags.Aggregate((c, n) => c + ';' + n);
                    _mailbox.IsSubscribed = subscriptions.Any(x => x.Name == mailbox.Name);

                    await context.SaveChangesAsync();
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        internal async Task<MailMessageContext[]> QueryAsync(string text) {
            using (var context = new DatabaseContext()) {
                var messages = await context.MailMessages
                    .Where(x => x.Subject.Contains(text) && x.MailboxId == Id)
                    .ToArrayAsync();

                return messages.Select(x => new MailMessageContext(this, x)).ToArray();
            }
        }

        public bool IsIdling {
            get { return _isIdling; }
            set {
                if (_isIdling == value) {
                    return;
                }

                _isIdling = value;
                RaisePropertyChanged(() => IsIdling);
            }
        }

        internal async void IdleAsync() {
            try {
                using (var connection = new ImapConnection { Security = _account.ImapSecurity }) {
                    connection.RemoteCertificateValidationFailed += (sender, e) => e.IsCanceled = false;
                    using (var auth = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                        using (var session = await auth.LoginAsync(_account.ImapUsername, _account.ImapPassword)) {
                            if (!connection.CanIdle) {
                                return;
                            }
                            var mailbox = await session.SelectAsync(Name);
                            try {
                                IsIdling = true;
                                mailbox.ChangeNotificationReceived += OnChangeNotificationReceived;
                                await mailbox.IdleAsync();
                            } finally {
                                mailbox.ChangeNotificationReceived -= OnChangeNotificationReceived;
                                IsIdling = false;
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private async void OnChangeNotificationReceived(object sender, EventArgs e) {
            try {
                await SyncMessagesAsync();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        internal async Task DeleteAsync() {
            try {
                using (var database = new DatabaseContext()) {
                    var messageModels = await database.MailMessages
                        .Where(x => x.MailboxId == Id)
                        .ToArrayAsync();

                    foreach (var message in messageModels) {
                        var m = message;
                        var mimeModels = await database.MimeMessages
                            .Where(x => x.MessageId == m.Id)
                            .ToArrayAsync();

                        foreach (var mime in mimeModels) {
                            database.MimeMessages.Remove(mime);
                        }
                    }

                    database.MailMessages.RemoveRange(messageModels);
                    database.Mailboxes.Attach(_mailbox);
                    database.Mailboxes.Remove(_mailbox);

                    await database.SaveChangesAsync();
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        public bool HasChildren {
            get { return Children.Any(); }
        }


        public bool IsLoadingMessages {
            get { return _isLoadingMessages; }
            set {
                if (_isLoadingMessages == value) {
                    return;
                }
                _isLoadingMessages = value;
                RaisePropertyChanged(() => IsLoadingMessages);
            }
        }

        #region Implementation of IMessageSource

        public async Task<IEnumerable<MailMessageContext>> GetMessagesAsync() {
            Application.Current.AssertBackgroundThread();

            IsLoadingMessages = true;

            IEnumerable<MailMessageModel> messages;

            using (var context = new DatabaseContext()) {
                if (ShowAllMessages) {
                    messages = await context.MailMessages
                        .Where(x => x.Type == MailType.Message)
                        .Where(x => x.MailboxId == _mailbox.Id)
                        .ToArrayAsync();
                }
                else {
                    messages = await context.MailMessages
                        .Where(x => x.Type == MailType.Message)
                        .Where(x => !x.Flags.Contains(MailboxFlags.Seen))
                        .Where(x => x.MailboxId == _mailbox.Id)
                        .ToArrayAsync();
                }
            }


            var contexts = messages
                .Select(x => new MailMessageContext(this, x))
                .ToArray();

            IsLoadingMessages = false;

            return contexts;
        }

        #endregion

        /// <summary>
        /// Moves messages from the trash mailbox back to the inbox.
        /// </summary>
        /// <param name="messages">The messages to move.</param>
        internal async void RestoreMessagesAsync(IList<MailMessageContext> messages) {
            try {
                if (messages.Count < 1) {
                    return;
                }

                var inbox = messages.First().Mailbox.Account.GetInbox();
                using (var connection = new ImapConnection { Security = _account.ImapSecurity }) {
                    using (var auth = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                        using (var session = await auth.LoginAsync(_account.ImapUsername, _account.ImapPassword)) {
                            var mailbox = await session.SelectAsync(Name);
                            if (Type == MailboxType.Trash) {
                                await mailbox.MoveMailsAsync(messages.Select(x => x.Uid).ToArray(), inbox.Name);
                            }
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
                        }
                    }
                    await database.SaveChangesAsync();
                }

                App.Context.NotifyMessagesRemoved(messages);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }
    }
}
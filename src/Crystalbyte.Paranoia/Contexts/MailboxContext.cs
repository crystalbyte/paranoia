#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Net;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI.Commands;
using Newtonsoft.Json;
using NLog;
using System.Text.RegularExpressions;

#endregion

namespace Crystalbyte.Paranoia {
    [DebuggerDisplay("Name = {Name}")]
    public sealed class MailboxContext : SelectionObject {
        private bool _isSyncing;
        private bool _isListingMailboxes;
        private bool _isAssignable;
        private readonly MailAccountContext _account;
        private readonly MailboxModel _mailbox;
        private readonly ObservableCollection<MailboxCandidateContext> _mailboxCandidates;
        private readonly AssignMailboxCommand _assignmentCommand;
        private MailboxCandidateContext _selectedCandidate;
        private bool _isLoadingMessage;
        private int _notSeenCount;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        internal MailboxContext(MailAccountContext account, MailboxModel mailbox) {
            _account = account;
            _mailbox = mailbox;
            _assignmentCommand = new AssignMailboxCommand(this);
            _mailboxCandidates = new ObservableCollection<MailboxCandidateContext>();
        }

        public event EventHandler AssignmentChanged;

        private void OnAssignmentChanged() {
            RaisePropertyChanged(() => IsAssigned);
            RaisePropertyChanged(() => IsAssignable);
            var handler = AssignmentChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

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

        internal async Task DeleteMessagesAsync(MailMessageContext[] messages, string trashFolder) {
            try {
                using (var connection = new ImapConnection { Security = _account.ImapSecurity }) {
                    using (var auth = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                        using (var session = await auth.LoginAsync(_account.ImapUsername, _account.ImapPassword)) {
                            var mailbox = await session.SelectAsync(Name);
                            if (Type == MailboxType.Trash) {
                                await mailbox.DeleteMailsAsync(messages.Select(x => x.Uid));
                            }
                            else {
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
                        }
                        catch (Exception ex) {
                            Logger.Error(ex);
                        }
                    }
                    await database.SaveChangesAsync();
                }

                App.Context.NotifyMessagesRemoved(messages);
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        internal async Task DropAssignmentAsync() {
            using (var context = new DatabaseContext()) {
                context.Mailboxes.Attach(_mailbox);

                _mailbox.Name = string.Empty;
                _mailbox.Flags = string.Empty;

                await context.SaveChangesAsync();
            }

            RaisePropertyChanged(() => IsAssigned);
            OnAssignmentChanged();
        }

        public bool IsAssigned {
            get { return !string.IsNullOrEmpty(Name); }
        }

        public ICommand AssignmentCommand {
            get { return _assignmentCommand; }
        }

        internal bool IsTrash {
            get { return Type == MailboxType.Trash; }
        }

        public bool IsAssignable {
            get { return _isAssignable; }
            set {
                if (_isAssignable == value) {
                    return;
                }
                _isAssignable = value;
                RaisePropertyChanged(() => IsAssignable);
                RaisePropertyChanged(() => Name);
            }
        }

        internal async Task PrepareManualAssignmentAsync() {
            IsListingMailboxes = true;
            try {
                var app = App.Composition.GetExport<AppContext>();
                var account = app.SelectedAccount;

                var mailboxes = await account.ListMailboxesAsync();
                _mailboxCandidates.Clear();
                _mailboxCandidates.AddRange(mailboxes
                    .Select(x => new MailboxCandidateContext(_account, x)));
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
            finally {
                IsListingMailboxes = false;
            }
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

        public MailboxCandidateContext SelectedCandidate {
            get { return _selectedCandidate; }
            set {
                if (_selectedCandidate == value) {
                    return;
                }
                _selectedCandidate = value;
                RaisePropertyChanged(() => SelectedCandidate);
            }
        }

        public IEnumerable<MailboxCandidateContext> MailboxCandidates {
            get { return _mailboxCandidates; }
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

        public bool IsSyncing {
            get { return _isSyncing; }
            set {
                if (_isSyncing == value) {
                    return;
                }
                _isSyncing = value;
                RaisePropertyChanged(() => IsSyncing);
            }
        }

        public bool IsLoadingMessage {
            get { return _isLoadingMessage; }
            set {
                if (_isLoadingMessage == value) {
                    return;
                }
                _isLoadingMessage = value;
                RaisePropertyChanged(() => IsLoadingMessage);
            }
        }

        public MailboxType Type {
            get { return _mailbox.Type; }
        }

        private Task<MailAccountModel> GetAccountAsync() {
            using (var context = new DatabaseContext()) {
                return context.MailAccounts.FindAsync(_mailbox.AccountId);
            }
        }

        protected override void OnSelectionChanged() {
            base.OnSelectionChanged();

            if (IsSelected)
                return;

            IsAssignable = false;
            _mailboxCandidates.Clear();
        }

        internal async Task SyncMessagesAsync() {
            Application.Current.AssertUIThread();

            if (!IsAssigned || IsSyncing) {
                return;
            }


            IsSyncing = true;

            try {
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

                await SaveContactsAsync(messages);
                await SaveMessagesAsync(messages);

                var contexts = messages.Where(x => x.Type == MailType.Message)
                    .Select(x => new MailMessageContext(this, x)).ToArray();

                if (IsInbox && contexts.Length > 0) {
                    var notification = new NotificationWindow(contexts);
                    notification.Show();
                }

                App.Context.NotifyMessagesAdded(contexts);

            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
            finally {
                IsSyncing = false;
            }
        }

        private async Task<IEnumerable<MailMessageModel>> SyncNonChallengesAsync(ImapMailbox mailbox, long uid) {
            var criteria = string.Format("{0}:* NOT HEADER \"{1}\" \"{2}\"", uid, ParanoiaHeaderKeys.Type, MailType.Challenge);
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

            var messages = new List<MailMessageModel>();
            foreach (var envelope in envelopes) {
                try {
                    var message = envelope.ToMailMessage(MailType.Message);
                    messages.Add(message);
                }
                catch (Exception ex) {
                    Logger.Error(ex);
                }
            }
            return messages;
        }

        private async Task<IEnumerable<MailMessageModel>> SyncChallengesAsync(ImapMailbox mailbox, long uid) {
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

            foreach (var envelope in envelopes) {
                try {
                    await ProcessChallengeAsync(envelope, mailbox);
                }
                catch (Exception ex) {
                    Logger.Error(ex);
                }
            }

            var messages = new List<MailMessageModel>();
            foreach (var envelope in envelopes) {
                try {
                    var message = envelope.ToMailMessage(MailType.Challenge);
                    messages.Add(message);
                }
                catch (Exception ex) {
                    Logger.Error(ex);
                }
            }
            return messages;
        }

        private async Task ProcessChallengeAsync(ImapEnvelope envelope, ImapMailbox mailbox) {
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

        private async Task RespondToChallengeAsync(string challenge) {
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
                var contacts = new List<MailContactModel>();
                using (var database = new DatabaseContext()) {
                    foreach (var message in messages) {
                        var m = message;
                        var contact = await database.MailContacts
                            .Where(x => x.Address == m.FromAddress)
                            .FirstOrDefaultAsync();

                        if (contact != null)
                            continue;

                        var model = new MailContactModel {
                            Address = m.FromAddress,
                            Name = m.FromName
                        };

                        database.MailContacts.Add(model);
                        await database.SaveChangesAsync();
                        contacts.Add(model);
                    }

                    App.Context.NotifyContactsAdded(contacts
                        .Select(x => new MailContactContext(x))
                        .ToList());
                }
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        internal async Task MarkAsNotSeenAsync(MailMessageContext[] messages) {
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
            }
            catch (Exception ex) {
                messages.ForEach(x => x.IsSeen = true);
                Logger.Error(ex);
            }
        }

        internal async Task MarkAsSeenAsync(MailMessageContext[] messages) {
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
            }
            catch (Exception ex) {
                messages.ForEach(x => x.IsSeen = false);
                Logger.Error(ex);
            }
        }

        internal async Task CountNotSeenAsync() {
            var contact = App.Context.SelectedContact;
            if (contact == null) {
                NotSeenCount = 0;
                return;
            }

            using (var context = new DatabaseContext()) {
                NotSeenCount = await context.MailMessages
                    .Where(x => x.Type == MailType.Message)
                    .Where(x => x.MailboxId == _mailbox.Id)
                    .Where(x => x.FromAddress == contact.Address)
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

        internal async Task AssignMostProbableAsync(List<ImapMailboxInfo> remoteMailboxes) {
            switch (Type) {
                case MailboxType.All:
                    await AssignAsync(_account.IsGmail
                        ? remoteMailboxes.SingleOrDefault(x => x.IsGmailAll)
                        : remoteMailboxes.SingleOrDefault(x => CultureInfo.CurrentCulture.CompareInfo
                            .IndexOf(x.Name, "all", CompareOptions.IgnoreCase) >= 0));
                    break;
                case MailboxType.Inbox:
                    await AssignAsync(remoteMailboxes.SingleOrDefault(
                        x => CultureInfo.CurrentCulture.CompareInfo
                            .IndexOf(x.Name, "inbox", CompareOptions.IgnoreCase) >= 0));
                    break;
                case MailboxType.Sent:
                    await AssignAsync(_account.IsGmail
                        ? remoteMailboxes.SingleOrDefault(x => x.IsGmailSent)
                        : remoteMailboxes.SingleOrDefault(x => CultureInfo.CurrentCulture.CompareInfo
                            .IndexOf(x.Name, "sent", CompareOptions.IgnoreCase) >= 0));
                    break;
                case MailboxType.Draft:
                    await AssignAsync(_account.IsGmail
                        ? remoteMailboxes.SingleOrDefault(x => x.IsGmailDraft)
                        : remoteMailboxes.SingleOrDefault(x => CultureInfo.CurrentCulture.CompareInfo
                            .IndexOf(x.Name, "draft", CompareOptions.IgnoreCase) >= 0));
                    break;
                case MailboxType.Trash:
                    await AssignAsync(_account.IsGmail
                        ? remoteMailboxes.SingleOrDefault(x => x.IsGmailTrash)
                        : remoteMailboxes.SingleOrDefault(x => CultureInfo.CurrentCulture.CompareInfo
                            .IndexOf(x.Name, "trash", CompareOptions.IgnoreCase) >= 0));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal async Task AssignAsync(ImapMailboxInfo mailbox) {
            try {
                // If no match has been found mailbox will be null.
                if (mailbox == null) {
                    return;
                }

                using (var context = new DatabaseContext()) {
                    context.Mailboxes.Attach(_mailbox);

                    _mailbox.Name = mailbox.Fullname;
                    _mailbox.Delimiter = mailbox.Delimiter;
                    _mailbox.Flags = mailbox.Flags.Aggregate((c, n) => c + ';' + n);

                    await context.SaveChangesAsync();
                    IsAssignable = false;
                    OnAssignmentChanged();
                }
            }
            catch (Exception ex) {
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

        internal void NotifyCandidateSelectionChanged() {
            _assignmentCommand.OnCanExecuteChanged();
        }

        internal async Task LoadMessagesForContactAsync(MailContactContext contact) {
            if (contact == null) {
                App.Context.ClearMessages();
                return;
            }

            try {
                IEnumerable<MailMessageModel> messages;
                using (var context = new DatabaseContext()) {
                    messages = await context.MailMessages
                        .Where(x => x.Type == MailType.Message)
                        .Where(x => x.MailboxId == _mailbox.Id)
                        .Where(x => x.FromAddress == contact.Address)
                        .ToArrayAsync();
                }

                // Check for active selection, since it might have changed while being async.
                if (!IsSelected) {
                    return;
                }

                App.Context.DisplayMessages(messages
                    .Select(x => new MailMessageContext(this, x)).ToArray());

                await CountNotSeenAsync();
            }
            catch (Exception ex) {
                Logger.Error(ex);
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
                                mailbox.ChangeNotificationReceived += OnChangeNotificationReceived;
                                await mailbox.IdleAsync();
                            }
                            finally {
                                mailbox.ChangeNotificationReceived -= OnChangeNotificationReceived;
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private async void OnChangeNotificationReceived(object sender, EventArgs e) {
            try {
                await SyncMessagesAsync();
            }
            catch (Exception ex) {
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
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        internal async Task LoadMessagesAsync() {
            try {
                IEnumerable<MailMessageModel> messages;
                using (var context = new DatabaseContext()) {
                    messages = await context.MailMessages
                        .Where(x => x.Type == MailType.Message)
                        .Where(x => x.MailboxId == _mailbox.Id)
                        .ToArrayAsync();
                }

                // Check for active selection, since it might have changed while being async.
                if (!IsSelected) {
                    return;
                }

                App.Context.DisplayMessages(messages
                    .Select(x => new MailMessageContext(this, x)).ToArray());

                await CountNotSeenAsync();
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }
    }
}
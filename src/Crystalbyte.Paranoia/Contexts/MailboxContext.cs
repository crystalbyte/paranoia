#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        

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
                            } else {
                                await mailbox.MoveMailsAsync(messages.Select(x => x.Uid), trashFolder);
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
                        } catch (Exception ex)
                        {
                            _logger.Error(ex.Message.ToString());
                        }
                    }
                    await database.SaveChangesAsync();
                }

                _account.AppContext.NotifyMessageCountChanged();
            } catch (Exception ex) {
                _logger.Error(ex.Message.ToString());
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
            } catch (Exception ex) {
                _logger.Error(ex.Message.ToString());
            } finally {
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

                            var criteria = string.Format("{0}:*", maxUid);
                            var uids = await mailbox.SearchAsync(criteria);
                            if (!uids.Any()) {
                                return;
                            }

                            var envelopes = (await mailbox.FetchEnvelopesAsync(uids)).ToArray();
                            if (envelopes.Length == 0) {
                                return;
                            }

                            if (envelopes.Length == 1 && envelopes.First().Uid == maxUid) {
                                return;
                            }

                            foreach (var envelope in envelopes) {
                                var isChallenge = false;
                                var responses = await mailbox.FetchHeadersAsync(new[] { envelope.Uid });
                                if (responses.ContainsKey(envelope.Uid)) {
                                    var headers = responses[envelope.Uid];
                                    isChallenge = headers.ContainsKey(ParanoiaHeaderKeys.Challenge);
                                    var isRelevant = envelope.InternalDate.HasValue
                                        && (DateTime.Now - envelope.InternalDate.Value) 
                                            < TimeSpan.FromHours(1);

                                    if (isChallenge && isRelevant) {
                                        try {
                                            await ProcessChallengeAsync(envelope, headers, mailbox);
                                        } catch (Exception ex) {
                                            _logger.Error(ex.Message.ToString());
                                        }
                                    }
                                }

                                var message = new MailMessageModel {
                                    EntryDate = envelope.InternalDate.HasValue
                                        ? envelope.InternalDate.Value
                                        : DateTime.Now,
                                    Subject = envelope.Subject,
                                    Flags = string.Join(";", envelope.Flags),
                                    Size = envelope.Size,
                                    Uid = envelope.Uid,
                                    MessageId = envelope.MessageId,
                                    FromAddress = envelope.From.Any()
                                        ? envelope.From.First().Address
                                        : string.Empty,
                                    FromName = envelope.From.Any()
                                        ? envelope.From.First().DisplayName
                                        : string.Empty,
                                    Type = isChallenge
                                        ? MailType.Challenge
                                        : MailType.Message
                                };

                                messages.Add(message);
                            }
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

            } catch (Exception ex) {
                _logger.Error(ex.Message.ToString());
            } finally {
                IsSyncing = false;
            }
        }

        private async Task ProcessChallengeAsync(ImapEnvelope envelope, HeaderCollection header, ImapMailbox mailbox) {
            var body = await mailbox.FetchMessageBodyAsync(envelope.Uid);
            var bytes = Encoding.UTF8.GetBytes(body);
            var message = new MailMessage(bytes);
            var attachments = message.FindAllAttachments();

            var token =
                attachments.FirstOrDefault(
                    x =>
                        string.Compare(x.FileName, ParanoiaFilenames.Token, StringComparison.InvariantCultureIgnoreCase) ==
                        0);
            if (token == null) {
                throw new Exception("OMFG BROKEN");
            }

            var nonce =
                attachments.FirstOrDefault(
                    x =>
                        string.Compare(x.FileName, ParanoiaFilenames.Nonce, StringComparison.InvariantCultureIgnoreCase) ==
                        0);
            if (nonce == null) {
                throw new Exception("OMFG BROKEN");
            }

            var publicKey =
                attachments.FirstOrDefault(
                    x =>
                        string.Compare(x.FileName, ParanoiaFilenames.PublicKey,
                            StringComparison.InvariantCultureIgnoreCase) == 0);
            if (publicKey == null) {
                throw new Exception("OMFG BROKEN");
            }

            var data = App.Context.KeyContainer.DecryptWithPrivateKey(token.Body, publicKey.Body, nonce.Body);
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
            } catch (Exception ex) {
                _logger.Error(ex.Message.ToString());
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
            } catch (Exception ex) {
                messages.ForEach(x => x.IsSeen = true);
                _logger.Error(ex.Message.ToString());
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
            } catch (Exception ex) {
                messages.ForEach(x => x.IsSeen = false);
                _logger.Error(ex.Message.ToString());
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
            } catch (Exception ex) {
                _logger.Error(ex.Message.ToString());
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
            } catch (Exception ex) {
                _logger.Error(ex.Message.ToString());
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
                _logger.Error(ex.Message.ToString());
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
            } catch (Exception ex) {
                _logger.Error(ex.Message.ToString());
            }
        }
    }
}
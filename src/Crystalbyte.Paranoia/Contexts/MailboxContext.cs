#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Windows;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.UI.Commands;
using System.Diagnostics;

#endregion

namespace Crystalbyte.Paranoia {
    [DebuggerDisplay("Name = {Name}")]
    public sealed class MailboxContext : SelectionObject {
        private bool _isSyncing;
        private Exception _lastException;
        private bool _isListingMailboxes;
        private bool _isAssignable;
        private ObservableCollection<MailMessageContext> _messages;
        private readonly MailAccountContext _account;
        private readonly MailboxModel _mailbox;
        private readonly ObservableCollection<MailboxCandidateContext> _mailboxCandidates;
        private readonly AssignMailboxCommand _assignMailboxCommand;
        private MailboxCandidateContext _selectedCandidate;
        private bool _isLoadingMessage;

        internal MailboxContext(MailAccountContext account, MailboxModel mailbox) {
            _account = account;
            _mailbox = mailbox;
            _assignMailboxCommand = new AssignMailboxCommand(this);

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

        public Exception LastException {
            get { return _lastException; }
            set {
                if (_lastException == value) {
                    return;
                }

                _lastException = value;
                RaisePropertyChanged(() => LastException);
            }
        }

        public ObservableCollection<MailMessageContext> Messages {
            get { return _messages; }
            set {
                if (Equals(_messages, value)) {
                    return;
                }
                _messages = value;
                RaisePropertyChanged(() => Messages);
            }
        }

        internal async Task DropAsync() {
            using (var context = new DatabaseContext()) {
                context.Mailboxes.Attach(_mailbox);

                _mailbox.Name = string.Empty;
                _mailbox.Flags = string.Empty;

                await context.SaveChangesAsync();
            }

            IsAssignable = true;
            RaisePropertyChanged(() => IsAssigned);
            OnAssignmentChanged();
        }

        public bool IsAssigned {
            get { return !string.IsNullOrEmpty(Name); }
        }

        public AssignMailboxCommand AssignMailboxCommand {
            get { return _assignMailboxCommand; }
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
                LastException = ex;
            } finally {
                IsListingMailboxes = false;
            }
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

        internal async Task SyncAsync() {
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

                            messages.AddRange(envelopes
                                .Select(envelope => new MailMessageModel {
                                    EntryDate = envelope.InternalDate.HasValue
                                        ? envelope.InternalDate.Value
                                        : DateTime.Now,
                                    Subject = envelope.Subject,
                                    Size = envelope.Size,
                                    Uid = envelope.Uid,
                                    MessageId = envelope.MessageId,
                                    FromAddress = envelope.From.Any()
                                        ? envelope.From.First().Address
                                        : string.Empty,
                                    FromName = envelope.From.Any()
                                        ? envelope.From.First().DisplayName
                                        : string.Empty
                                }));
                        }
                    }
                }

                await SaveMessagesToDatabaseAsync(messages);
                AppendMessages(messages);
            } catch (Exception ex) {
                LastException = ex;
            } finally {
                IsSyncing = false;
            }
        }

        private void AppendMessages(IEnumerable<MailMessageModel> messages) {
            var contexts = messages.Select(x => new MailMessageContext(x));
            if (Messages == null) {
                Messages = new ObservableCollection<MailMessageContext>(contexts);
            } else {
                Messages.AddRange(contexts);
            }
        }

        internal async Task LoadMessagesFromDatabaseAsync() {
            try {
                IEnumerable<MailMessageModel> messages;
                using (var context = new DatabaseContext()) {
                    messages = await context.MailMessages
                        .Where(x => x.MailboxId == _mailbox.Id)
                        .ToArrayAsync();
                }

                // Check for active selection, since it might have changed while being async.
                if (!IsSelected) {
                    return;
                }
                Messages = new ObservableCollection<MailMessageContext>(
                    messages.Select(x => new MailMessageContext(x)));
            } catch (Exception ex) {
                LastException = ex;
            }
        }

        private async Task SaveMessagesToDatabaseAsync(IEnumerable<MailMessageModel> messages) {
            using (var context = new DatabaseContext()) {
                context.Mailboxes.Attach(_mailbox);
                _mailbox.Messages.AddRange(messages);
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
                    await AssignAsync(remoteMailboxes.SingleOrDefault(
                        x => x.IsGmailAll || CultureInfo.CurrentCulture.CompareInfo
                            .IndexOf(x.Name, "all", CompareOptions.IgnoreCase) >= 0));
                    break;
                case MailboxType.Inbox:
                    await AssignAsync(remoteMailboxes.SingleOrDefault(
                        x => CultureInfo.CurrentCulture.CompareInfo
                            .IndexOf(x.Name, "inbox", CompareOptions.IgnoreCase) >= 0));
                    break;
                case MailboxType.Sent:
                    await AssignAsync(remoteMailboxes.SingleOrDefault(
                        x => x.IsGmailSent || CultureInfo.CurrentCulture.CompareInfo
                            .IndexOf(x.Name, "sent", CompareOptions.IgnoreCase) >= 0));
                    break;
                case MailboxType.Draft:
                    await AssignAsync(remoteMailboxes.SingleOrDefault(
                        x => x.IsGmailDraft || CultureInfo.CurrentCulture.CompareInfo
                            .IndexOf(x.Name, "draft", CompareOptions.IgnoreCase) >= 0));
                    break;
                case MailboxType.Trash:
                    await AssignAsync(remoteMailboxes.SingleOrDefault(
                        x => x.IsGmailTrash || CultureInfo.CurrentCulture.CompareInfo
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
                LastException = ex;
            }
        }

        internal void NotifyCandidateSelectionChanged() {
            _assignMailboxCommand.OnCanExecuteChanged();
        }
    }
}
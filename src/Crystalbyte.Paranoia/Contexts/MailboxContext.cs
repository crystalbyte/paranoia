#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.UI.Commands;

#endregion

namespace Crystalbyte.Paranoia {
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
        private readonly DropMailboxCommand _dropMailboxCommand;
        private MailboxCandidateContext _selectedCandidate;
        private bool _isLoadingMessage;

        internal MailboxContext(MailAccountContext account, MailboxModel mailbox) {
            _account = account;
            _mailbox = mailbox;
            _assignMailboxCommand = new AssignMailboxCommand(this);
            _dropMailboxCommand = new DropMailboxCommand(this);
            _mailboxCandidates = new ObservableCollection<MailboxCandidateContext>();
        }

        public event EventHandler AssignmentChanged;

        private void OnAssignmentChanged() {
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

        internal Task DropAsync() {
            return Task.Factory.StartNew(() => {
                lock (_mailbox) {
                    using (var context = new DatabaseContext()) {
                        context.Mailboxes.Attach(_mailbox);

                        _mailbox.Name = string.Empty;
                        _mailbox.Flags = string.Empty;

                        context.SaveChanges();
                    }
                    IsAssignable = true;
                    OnAssignmentChanged();
                }
            });
        }

        public bool IsAssigned {
            get { return !string.IsNullOrEmpty(Name); }
        }

        public AssignMailboxCommand AssignMailboxCommand {
            get { return _assignMailboxCommand; }
        }

        public DropMailboxCommand DropMailboxCommand {
            get { return _dropMailboxCommand; }
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

        public async Task DownloadMessageAsync() {
            try {
                var account = await GetAccountAsync();
                using (var connection = new ImapConnection { Security = account.ImapSecurity }) {
                    connection.RemoteCertificateValidationFailed += (sender, e) => e.IsCancelled = false;
                    using (var auth = await connection.ConnectAsync(account.ImapHost, account.ImapPort)) {
                        using (var session = await auth.LoginAsync(account.ImapUsername, account.ImapPassword)) {
                            //var mailbox = await session.SelectAsync(name);
                        }
                    }
                }
            }
            catch (Exception) {
                
                throw;
            }
            
        }

        internal async Task PrepareManualAssignmentAsync() {
            IsListingMailboxes = true;
            try {

                var source = App.Composition.GetExport<MailAccountSelectionSource>();
                var account = source.Selection.First();

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
            return Task.Factory.StartNew(() => {
                lock (_mailbox) {
                    using (var context = new DatabaseContext()) {
                        context.Mailboxes.Attach(_mailbox);
                        return _mailbox.Account;
                    }
                }
            });
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
                    connection.RemoteCertificateValidationFailed += (sender, e) => e.IsCancelled = false;
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

                            messages.AddRange(envelopes.Select(envelope => new MailMessageModel {
                                EntryDate = envelope.InternalDate.HasValue
                                    ? envelope.InternalDate.Value
                                    : DateTime.Now,
                                Subject = envelope.Subject,
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
                var messages = await Task.Factory.StartNew(() => {
                    lock (_mailbox) {
                        using (var context = new DatabaseContext()) {
                            context.Mailboxes.Attach(_mailbox);
                            return _mailbox.Messages.ToArray();
                        }
                    }
                });

                Messages = new ObservableCollection<MailMessageContext>(
                    messages.Select(x => new MailMessageContext(x)));

            } catch (Exception ex) {
                LastException = ex;
            }
        }

        private Task SaveMessagesToDatabaseAsync(IEnumerable<MailMessageModel> messages) {
            return Task.Factory.StartNew(() => {
                lock (_mailbox) {
                    try {
                        using (var context = new DatabaseContext()) {
                            context.Mailboxes.Attach(_mailbox);
                            _mailbox.Messages.AddRange(messages);
                            context.SaveChanges();
                        }
                    } catch (Exception ex) {
                        LastException = ex;
                    }
                }
            });
        }

        private Task<Int64> GetMaxUidAsync() {
            return Task.Factory.StartNew(() => {
                lock (_mailbox) {
                    using (var context = new DatabaseContext()) {
                        context.Mailboxes.Attach(_mailbox);
                        return !_mailbox.Messages.Any()
                            ? 1
                            : _mailbox.Messages.Max(x => x.Uid);
                    }
                }
            });
        }

        internal async Task AssignMostProbableAsync(List<ImapMailboxInfo> remoteMailboxes) {
            switch (Type) {
                case MailboxType.Inbox:
                    await AssignAsync(remoteMailboxes.SingleOrDefault(
                        x => CultureInfo.CurrentCulture.CompareInfo
                            .IndexOf(x.Name, "inbox", CompareOptions.IgnoreCase) >= 0));
                    break;
                case MailboxType.Sent:
                    await AssignAsync(remoteMailboxes.SingleOrDefault(
                        x => CultureInfo.CurrentCulture.CompareInfo
                            .IndexOf(x.Name, "sent", CompareOptions.IgnoreCase) >= 0));
                    break;
                case MailboxType.Draft:
                    await AssignAsync(remoteMailboxes.SingleOrDefault(
                        x => CultureInfo.CurrentCulture.CompareInfo
                            .IndexOf(x.Name, "draft", CompareOptions.IgnoreCase) >= 0));
                    break;
                case MailboxType.Trash:
                    await AssignAsync(remoteMailboxes.SingleOrDefault(
                        x => CultureInfo.CurrentCulture.CompareInfo
                            .IndexOf(x.Name, "trash", CompareOptions.IgnoreCase) >= 0));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal Task AssignAsync(ImapMailboxInfo mailbox) {
            return Task.Factory.StartNew(() => {
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

                        context.SaveChangesAsync();
                        IsAssignable = false;
                        OnAssignmentChanged();
                    }
                } catch (Exception
                    ex) {
                    LastException = ex;
                }
            });
        }

        internal void NotifyCandidateSelectionChanged() {
            _assignMailboxCommand.OnCanExecuteChanged();
        }
    }
}
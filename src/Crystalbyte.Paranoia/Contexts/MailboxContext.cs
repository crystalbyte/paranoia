using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using System.Collections.Generic;

namespace Crystalbyte.Paranoia {
    public sealed class MailboxContext : SelectionObject {
        private Exception _lastException;
        private readonly MailboxModel _mailbox;
        private IEnumerable<MailMessageContext> _messages;

        internal MailboxContext(MailboxModel mailbox) {
            _mailbox = mailbox;
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

        public IEnumerable<MailMessageContext> Messages {
            get { return _messages; }
            set {
                if (Equals(_messages, value)) {
                    return;
                }
                _messages = value;
                RaisePropertyChanged(() => Messages);
            }
        }

        public bool IsAssigned {
            get { return !string.IsNullOrEmpty(Name); }
        }

        public MailboxType Type {
            get { return _mailbox.Type; }
        }

        private Task<MailAccountModel> GetAccountAsync() {
            return Task.Factory.StartNew(() => {
                using (var context = new DatabaseContext()) {
                    context.Mailboxes.Attach(_mailbox);
                    return _mailbox.Account;
                }
            });
        }

        protected async override void OnSelectionChanged() {
            base.OnSelectionChanged();

            if (IsSelected) {
                await SyncAsync();    
            }
        }

        internal async Task SyncAsync() {
            if (!IsAssigned) {
                return;
            }

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

                            var envelopes = (await mailbox.FetchEnvelopesAsync(uids)).ToArray();
                            if (envelopes.Length == 0) {
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
                await LoadMessagesFromDatabaseAsync();

                if (IsSelected) {
                    var app = App.Composition.GetExport<AppContext>();
                    app.UpdateMessages();
                }

            } catch (Exception ex) {
                LastException = ex;
            }
        }

        private async Task LoadMessagesFromDatabaseAsync() {
            try {
                var messages = await Task.Factory.StartNew(() => {
                    using (var context = new DatabaseContext()) {
                        context.Mailboxes.Attach(_mailbox);
                        return _mailbox.Messages.ToArray();
                    }
                });

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
            return Task.Factory.StartNew(() => {
                using (var context = new DatabaseContext()) {
                    context.Mailboxes.Attach(_mailbox);
                    return !_mailbox.Messages.Any()
                        ? 1
                        : _mailbox.Messages.Max(x => x.Uid);
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
                }
            } catch (Exception ex) {
                LastException = ex;
            }
        }
    }
}

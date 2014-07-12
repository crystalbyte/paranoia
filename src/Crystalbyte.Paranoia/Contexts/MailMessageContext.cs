using System;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;

namespace Crystalbyte.Paranoia {
    public class MailMessageContext : SelectionObject {
        private readonly MailMessageModel _message;
        private Exception _lastException;
        private string _html;

        public MailMessageContext(MailMessageModel message) {
            _message = message;
        }

        public long Uid {
            get { return _message.Uid; }
        }

        public string Subject {
            get { return _message.Subject; }
        }

        public DateTime EntryDate {
            get { return _message.EntryDate; }
        }

        public string FromName {
            get { return _message.FromName; }
        }

        public string FromAddress {
            get { return _message.FromAddress; }
        }

        public string Html {
            get { return _html; }
            set {
                if (_html == value) {
                    return;
                }
                _html = value;
                RaisePropertyChanged(() => Html);
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

        private Task<MailAccountModel> GetAccountAsync() {
            return Task.Factory.StartNew(() => {
                lock (_message) {
                    using (var context = new DatabaseContext()) {
                        context.MailMessages.Attach(_message);
                        return _message.Mailbox.Account;
                    }
                }
            });
        }

        public async Task DownloadMessageAsync() {
            try {
                var account = await GetAccountAsync();
                var mailbox = await GetMailboxAsync();
                using (var connection = new ImapConnection { Security = account.ImapSecurity }) {
                    connection.RemoteCertificateValidationFailed += (sender, e) => e.IsCancelled = false;
                    using (var auth = await connection.ConnectAsync(account.ImapHost, account.ImapPort)) {
                        using (var session = await auth.LoginAsync(account.ImapUsername, account.ImapPassword)) {
                            var folder = await session.SelectAsync(mailbox.Name);
                            Html = await folder.FetchMessageBodyAsync(Uid);
                        }
                    }
                }
            } catch (Exception ex) {
                LastException = ex;
            }
        }

        private Task<MailboxModel> GetMailboxAsync() {
            return Task.Factory.StartNew(() => {
                lock (_message) {
                    using (var context = new DatabaseContext()) {
                        context.MailMessages.Attach(_message);
                        return _message.Mailbox;
                    }
                }
            });
        }
    }
}

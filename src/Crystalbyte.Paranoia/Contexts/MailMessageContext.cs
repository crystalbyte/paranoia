using System;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;

namespace Crystalbyte.Paranoia {
    public class MailMessageContext : SelectionObject {
        private int _load;
        private string _html;
        private long _bytesReceived;
        private Exception _lastException;
        private object _databaseMutex;
        private readonly MailMessageModel _message;

        public MailMessageContext(MailMessageModel message) {
            _message = message;
            _databaseMutex = new object();
        }

        public long Uid {
            get { return _message.Uid; }
        }

        public long Size {
            get { return _message.Size; }
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

        public Task<string> LoadMimeFromDatabaseAsync() {
            return Task.Factory.StartNew(() => {
                lock (_databaseMutex) {
                    using (var context = new DatabaseContext()) {
                        context.MailMessages.Attach(_message);
                        var message =  _message.MimeMessages.FirstOrDefault();
                        return message != null ? message.Data : string.Empty;
                    }
                }
            });
        }

        public bool IsLoading {
            get {
                return _load > 0;
            }
        }
        
        private void IncrementLoad() {
            _load++;
            RaisePropertyChanged(() => IsLoading);
        }

        private void DecrementLoad() {
            _load--;
            RaisePropertyChanged(() => IsLoading);
        }

        public long BytesReceived {
            get {
                return _bytesReceived;
            }
            set {
                if (_bytesReceived == value) {
                    return;
                }
                _bytesReceived = value;
                RaisePropertyChanged(() => BytesReceived);
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
                lock (_databaseMutex) {
                    using (var context = new DatabaseContext()) {
                        context.MailMessages.Attach(_message);
                        return _message.Mailbox.Account;
                    }
                }
            });
        }

        private async Task<string> FetchMimeAsync() {
            var account = await GetAccountAsync();
            var mailbox = await GetMailboxAsync();

            using (var connection = new ImapConnection { Security = account.ImapSecurity }) {
                connection.RemoteCertificateValidationFailed += (sender, e) => e.IsCancelled = false;
                using (var auth = await connection.ConnectAsync(account.ImapHost, account.ImapPort)) {
                    using (var session = await auth.LoginAsync(account.ImapUsername, account.ImapPassword)) {
                        var folder = await session.SelectAsync(mailbox.Name);

                        folder.ProgressChanged += OnProgressChanged;
                        var mime = await folder.FetchMessageBodyAsync(Uid);
                        folder.ProgressChanged -= OnProgressChanged;

                        return mime;
                    }
                }
            }
        }

        private void OnProgressChanged(object sender, ProgressChangedEventArgs e) {
            BytesReceived = e.ByteCount;
        }

        public async Task<string> DownloadMessageAsync() {
            IncrementLoad();

            try {
                var mime = await FetchMimeAsync();
                await Task.Factory.StartNew(() => {
                    lock (_databaseMutex) {
                        using (var context = new DatabaseContext()) {
                            context.MailMessages.Attach(_message);
                            var mimeMessage = new MimeMessageModel {
                                Data = mime
                            };

                            _message.MimeMessages.Add(mimeMessage);
                            context.SaveChanges();
                        }
                    }
                });

                return mime;
            }
            catch (Exception ex) {
                LastException = ex;
            }
            finally {
                DecrementLoad();
            }

            return string.Empty;
        }

        private Task<MailboxModel> GetMailboxAsync() {
            return Task.Factory.StartNew(() => {
                lock (_databaseMutex) {
                    using (var context = new DatabaseContext()) {
                        context.MailMessages.Attach(_message);
                        return _message.Mailbox;
                    }
                }
            });
        }
    }
}

﻿#region Using directives

using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;

#endregion

namespace Crystalbyte.Paranoia {
    [DebuggerDisplay("Subject = {Subject}, Address = {FromAddress}")]
    public class MailMessageContext : SelectionObject {
        private int _load;
        private long _bytesReceived;
        private Exception _lastException;
        private readonly MailMessageModel _message;

        public MailMessageContext(MailMessageModel message) {
            _message = message;
        }
        public long Id {
            get { return _message.Id; }
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

        public bool IsSeen {
            get { return HasFlag(MailboxFlags.Seen); }
            set {
                if (HasFlag(MailboxFlags.Seen) == value) {
                    return;
                }

                if (value) {
                    WriteFlag(MailboxFlags.Seen);
                }
                else {
                    DropFlag(MailboxFlags.Seen);
                }

                RaisePropertyChanged(() => IsSeen);
                RaisePropertyChanged(() => IsNotSeen);
            }
        }

        public bool IsNotSeen {
            get { return !IsSeen; }
        }

        private void DropFlag(string flag) {
            var flags = _message.Flags.Split(';').ToList();
            flags.Remove(flag);

            _message.Flags = string.Join(";", flags);
        }

        private void WriteFlag(string flag) {
            var flags = _message.Flags.Split(';').ToList();
            flags.Add(flag);

            _message.Flags = string.Join(";", flags);
        }

        private bool HasFlag(string flag) {
            return _message.Flags.ContainsIgnoreCase(flag);
        }

        public async Task<string> LoadMimeFromDatabaseAsync() {
            IncrementLoad();
            try {
                using (var context = new DatabaseContext()) {
                    context.MailMessages.Attach(_message);
                    var message = await context.MimeMessages
                        .FirstOrDefaultAsync(x => x.MessageId == _message.Id);
                    return message != null ? message.Data : string.Empty;
                }
            } catch (Exception ex) {
                LastException = ex;
            } finally {
                DecrementLoad();
            }

            return string.Empty;
        }

        public bool IsLoading {
            get { return _load > 0; }
        }

        public bool IsNotLoading {
            get { return _load == 0; }
        }

        private void IncrementLoad() {
            _load++;
            RaisePropertyChanged(() => IsLoading);
            RaisePropertyChanged(() => IsNotLoading);
        }

        private void DecrementLoad() {
            _load--;
            RaisePropertyChanged(() => IsLoading);
            RaisePropertyChanged(() => IsNotLoading);
        }

        public long BytesReceived {
            get { return _bytesReceived; }
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

        private static Task<MailAccountModel> GetAccountAsync(MailboxModel mailbox) {
            using (var context = new DatabaseContext()) {
                return context.MailAccounts.FindAsync(mailbox.AccountId);
            }
        }

        private async Task<string> FetchMimeAsync() {
            var mailbox = await GetMailboxAsync();
            var account = await GetAccountAsync(mailbox);

            using (var connection = new ImapConnection { Security = account.ImapSecurity }) {
                connection.RemoteCertificateValidationFailed += (sender, e) => e.IsCanceled = false;
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
            //Debug.WriteLine(e.ByteCount);
            BytesReceived = e.ByteCount;
        }

        internal async Task<string> DownloadMessageAsync() {
            IncrementLoad();
            try {
                var mime = await FetchMimeAsync();
                using (var context = new DatabaseContext()) {
                    context.MailMessages.Attach(_message);
                    var mimeMessage = new MimeMessageModel {
                        Data = mime
                    };

                    _message.MimeMessages.Add(mimeMessage);
                    await context.SaveChangesAsync();
                }
                return mime;
            } catch (Exception ex) {
                LastException = ex;
            } finally {
                DecrementLoad();
            }

            return string.Empty;
        }

        private Task<MailboxModel> GetMailboxAsync() {
            using (var context = new DatabaseContext()) {
                return context.Mailboxes.FindAsync(_message.MailboxId);
            }
        }
    }
}
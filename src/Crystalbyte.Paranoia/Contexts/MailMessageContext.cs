#region Using directives

using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using NLog;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion

namespace Crystalbyte.Paranoia {
    [DebuggerDisplay("Subject = {Subject}, Address = {FromAddress}")]
    public class MailMessageContext : SelectionObject {
        private int _load;
        private long _bytesReceived;
        private readonly MailboxContext _mailbox;
        private readonly MailMessageModel _message;
        private readonly ObservableCollection<AttachmentContext> _attachments;
        private long _downloadProgress;
        private bool _isLocal;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        internal MailMessageContext(MailboxContext mailbox, MailMessageModel message) {
            _mailbox = mailbox;
            _message = message;
            _attachments = new ObservableCollection<AttachmentContext>();
        }

        public bool IsLocallyAvailable {
            get { return _isLocal; }
            set {
                if (_isLocal == value) {
                    return;
                }

                _isLocal = value;
                RaisePropertyChanged(() => IsLocallyAvailable);
            }
        }

        public bool IsRecycled {
            get { return _message == null; }
        }

        public ICollection<AttachmentContext> Attachments {
            get { return _attachments; }
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

        public MailboxContext Mailbox {
            get { return _mailbox; }
        }

        public bool IsSubjectNilOrEmpty {
            get { return Subject == "NIL" || string.IsNullOrEmpty(Subject); }
        }

        public long DownloadProgress {
            get { return _downloadProgress; }
            set {
                if (_downloadProgress == value) {
                    return;
                }
                _downloadProgress = value;
                RaisePropertyChanged(() => DownloadProgress);
            }
        }


        public bool IsSeen {
            get { return HasFlag(MailboxFlags.Seen); }
            set {
                if (HasFlag(MailboxFlags.Seen) == value) {
                    return;
                }

                if (value) {
                    WriteFlag(MailboxFlags.Seen);
                } else {
                    DropFlag(MailboxFlags.Seen);
                }

                RaisePropertyChanged(() => IsSeen);
                RaisePropertyChanged(() => IsNotSeen);
                OnSeenStatusChanged();
            }
        }

        private async void OnSeenStatusChanged() {
            await SaveFlagsToDatabaseAsync();
        }

        private async Task SaveFlagsToDatabaseAsync() {
            try {
                using (var database = new DatabaseContext()) {
                    var message = await database.MailMessages.FindAsync(_message.Id);
                    message.Flags = _message.Flags;
                    await database.SaveChangesAsync();
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        public bool IsNotSeen {
            get { return !IsSeen; }
        }

        private void DropFlag(string flag) {
            var flags = _message.Flags.Split(';').ToList();
            flags.RemoveAll(x => x.Equals(flag, StringComparison.InvariantCultureIgnoreCase));

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

                        folder.ByteCountChanged += OnProgressChanged;
                        var mime = await folder.FetchMessageBodyAsync(Uid);
                        folder.ByteCountChanged -= OnProgressChanged;

                        return mime;
                    }
                }
            }
        }

        private void OnProgressChanged(object sender, ProgressChangedEventArgs e) {
            BytesReceived = e.ByteCount;
            DownloadProgress = (BytesReceived * 100 / Size);
            
            // Message may differ from actual sizes due to encoding.
            if (DownloadProgress > 100) {
                DownloadProgress = 100;
            }
        }

        internal async Task<string> DownloadMessageAsync() {
            IncrementLoad();
            try {
                var mime = await FetchMimeAsync();
                using (var context = new DatabaseContext()) {
                    var message = await context.MailMessages.FindAsync(_message.Id);
                    var mimeMessage = new MimeMessageModel {
                        Data = mime
                    };

                    message.MimeMessages.Add(mimeMessage);
                    await context.SaveChangesAsync();
                }
                return mime;
            } finally {
                DecrementLoad();
            }
        }

        internal Task<bool> GetIsMimeLoadedAsync() {
            using (var database = new DatabaseContext()) {
                return database.MimeMessages.Where(x => x.MessageId == Id).AnyAsync();
            }
        }

        private Task<MailboxModel> GetMailboxAsync() {
            using (var context = new DatabaseContext()) {
                return context.Mailboxes.FindAsync(_message.MailboxId);
            }
        }
    }
}
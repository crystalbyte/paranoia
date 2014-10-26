#region Using directives

using System;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Crystalbyte.Paranoia.Cryptography;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Mail.Mime;
using Crystalbyte.Paranoia.UI.Commands;
using NLog;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion

namespace Crystalbyte.Paranoia {
    [DebuggerDisplay("Subject = {Subject}, Address = {FromAddress}")]
    public class MailMessageContext : SelectionObject {

        #region Private Fields

        private int _load;
        private bool _isLocal;
        private long _bytesReceived;
        private int _progressChanged;
        private readonly ICommand _elevateTrustLevelCommand;
        private readonly MailboxContext _mailbox;
        private readonly MailMessageModel _message;

        private readonly ObservableCollection<AttachmentContext> _attachments;
        private bool _isSourceTrusted;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        internal MailMessageContext(MailboxContext mailbox, MailMessageModel message) {
            _mailbox = mailbox;
            _message = message;
            _attachments = new ObservableCollection<AttachmentContext>();
            _elevateTrustLevelCommand = new RelayCommand(OnElevateTrustCommand);
        }

        private async void OnElevateTrustCommand(object obj) {
            try {
                await TrustSourceAsync();
                await App.Context.RefreshMessageSelectionAsync(this);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        #endregion

        #region Properties

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

        public bool IsSourceTrusted {
            get { return _isSourceTrusted; }
            set {
                if (_isSourceTrusted == value) {
                    return;
                }
                _isSourceTrusted = value;
                RaisePropertyChanged(() => IsSourceTrusted);
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

        public ICommand ElevateTrustLevelCommand {
            get { return _elevateTrustLevelCommand; }
        }

        public MailboxContext Mailbox {
            get { return _mailbox; }
        }

        public bool IsSubjectNilOrEmpty {
            get { return Subject == "NIL" || string.IsNullOrEmpty(Subject); }
        }

        public bool IsFlagged {
            get { return HasFlag(MailMessageFlags.Flagged); }
            set {
                if (HasFlag(MailMessageFlags.Flagged) == value) {
                    return;
                }

                if (value) {
                    WriteFlag(MailMessageFlags.Flagged);
                } else {
                    DropFlag(MailMessageFlags.Flagged);
                }

                RaisePropertyChanged(() => IsFlagged);
                RaisePropertyChanged(() => IsNotFlagged);
                OnFlaggedStatusChanged();
            }
        }

        public bool IsSeen {
            get { return HasFlag(MailMessageFlags.Seen); }
            set {
                if (HasFlag(MailMessageFlags.Seen) == value) {
                    return;
                }

                if (value) {
                    WriteFlag(MailMessageFlags.Seen);
                } else {
                    DropFlag(MailMessageFlags.Seen);
                }

                RaisePropertyChanged(() => IsSeen);
                RaisePropertyChanged(() => IsNotSeen);
                OnSeenStatusChanged();
            }
        }

        #endregion

        private async void OnSeenStatusChanged() {
            await SaveFlagsToDatabaseAsync();
        }

        private async void OnFlaggedStatusChanged() {
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

        public bool IsNotFlagged {
            get { return !IsFlagged; }
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
                RaisePropertyChanged(() => Size);
            }
        }

        private static Task<MailAccountModel> GetAccountAsync(MailboxModel mailbox) {
            using (var context = new DatabaseContext()) {
                return context.MailAccounts.FindAsync(mailbox.AccountId);
            }
        }

        private async Task<byte[]> FetchMimeAsync() {
            var mailbox = await GetMailboxAsync();
            var account = await GetAccountAsync(mailbox);

            using (var connection = new ImapConnection { Security = account.ImapSecurity }) {
                connection.RemoteCertificateValidationFailed += (sender, e) => e.IsCanceled = false;
                using (var auth = await connection.ConnectAsync(account.ImapHost, account.ImapPort)) {
                    using (var session = await auth.LoginAsync(account.ImapUsername, account.ImapPassword)) {
                        var folder = await session.SelectAsync(mailbox.Name);

                        folder.ByteCountChanged += OnByteCountChanged;
                        var mime = await folder.FetchMessageBodyAsync(Uid);
                        folder.ByteCountChanged -= OnByteCountChanged;

                        return mime;
                    }
                }
            }
        }

        private void OnByteCountChanged(object sender, ProgressChangedEventArgs e) {
            BytesReceived = e.ByteCount;
            var percentage = (Convert.ToDouble(e.ByteCount) / Convert.ToDouble(Size)) * 100;

            // Total bytes and the actual size may differ due to encoding and compression.
            if (percentage > 100) {
                percentage = 100;
            }

            ProgressChanged = Convert.ToInt32(percentage);
        }

        public int ProgressChanged {
            get { return _progressChanged; }
            set {
                if (Math.Abs(_progressChanged - value) < double.Epsilon) {
                    return;
                }
                _progressChanged = value;
                RaisePropertyChanged(() => ProgressChanged);
            }
        }

        internal async Task<byte[]> DownloadMessageAsync() {
            IncrementLoad();
            try {
                var mime = await FetchMimeAsync();
                using (var context = new DatabaseContext()) {
                    var message = await context.MailMessages.FindAsync(_message.Id);

                    var mimeMessage = new MimeMessageModel {
                        Data = await FindRelevantPartAsync(mime)
                    };

                    message.MimeMessages.Add(mimeMessage);
                    await context.SaveChangesAsync();
                }
                return mime;

            } finally {
                DecrementLoad();
            }
        }

        private static async Task<byte[]> FindRelevantPartAsync(byte[] mime) {
            var reader = new MailMessageReader(mime);
            var encryptedParts = reader.FindAllMessagePartsWithMediaType(MediaTypes.EncryptedMime);
            if (encryptedParts == null || encryptedParts.Count == 0) {
                return mime;
            }

            foreach (var part in encryptedParts) {
                var result = await DecryptPart(reader, part);
                if (result.IsSuccessful) {
                    return result.Bytes;
                }
            }

            throw new MessageDecryptionFailedException();
        }

        private static async Task<DecryptionResult> DecryptPart(MailMessageReader reader, MessagePart part) {
            try {
                var address = reader.Headers.From.Address;
                var publicKey = reader.Headers.UnknownHeaders.Get(ParanoiaHeaderKeys.PublicKey);

                byte[] messageBytes, nonceBytes;
                using (var r = new BinaryReader(new MemoryStream(part.Body))) {
                    nonceBytes = r.ReadBytes(PublicKeyCrypto.NonceSize);
                    messageBytes = r.ReadBytes(part.Body.Length - nonceBytes.Length);
                }

                using (var database = new DatabaseContext()) {
                    var contact = await database.MailContacts.FirstOrDefaultAsync(x => x.Address == address);
                    if (contact == null) {
                        throw new Exception("Contact not found exception.");
                    }

                    var keys = await database.PublicKeys.Where(x => x.ContactId == contact.Id).ToArrayAsync();
                    if (keys.All(x => string.Compare(publicKey, x.Data, StringComparison.InvariantCulture) != 0)) {
                        var ownKey = Convert.ToBase64String(App.Context.KeyContainer.PublicKey);
                        if (string.Compare(ownKey, publicKey, StringComparison.Ordinal) != 0) {
                            return new DecryptionResult {
                                IsSuccessful = false
                            };
                        }
                    }
                }

                var keyBytes = Convert.FromBase64String(publicKey);
                return new DecryptionResult {
                    IsSuccessful = true,
                    Bytes = App.Context.KeyContainer.DecryptWithPrivateKey(messageBytes, keyBytes, nonceBytes)
                };

            } catch (Exception ex) {
                Logger.Error(ex);
                return new DecryptionResult {
                    IsSuccessful = false
                };
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

        internal async Task TrustSourceAsync() {
            using (var database = new DatabaseContext()) {
                var contact = await database.MailContacts
                    .FirstAsync(x => x.Address == FromAddress);

                contact.IsTrusted = true;
                await database.SaveChangesAsync();
            }
        }

        internal async Task UpdateTrustLevelAsync() {
            using (var database = new DatabaseContext()) {
                var contact = await database.MailContacts
                    .FirstOrDefaultAsync(x => x.Address == FromAddress);

                IsSourceTrusted = contact != null && contact.IsTrusted;
            }
        }
    }
}
#region Using directives

using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.UI.Commands;
using NLog;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion

namespace Crystalbyte.Paranoia {
    [DebuggerDisplay("Subject = {Subject}, Address = {FromAddress}")]
    public class MailMessageContext : SelectionObject, IInspectable {

        #region Private Fields

        private int _load;
        private long _bytesReceived;
        private double _progress;
        private bool _isSourceTrusted;
        private bool _hasExternals;
        private MailContactContext _from;

        private readonly MailboxContext _mailbox;
        private readonly MailMessageModel _message;
        private readonly ObservableCollection<MailContactContext> _to;
        private readonly ObservableCollection<MailContactContext> _cc;
        private readonly ObservableCollection<AttachmentContext> _attachments;
        private readonly ICommand _elevateTrustLevelCommand;
        private bool _isLoaded;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        internal MailMessageContext(MailboxContext mailbox, MailMessageModel message) {
            _mailbox = mailbox;
            _message = message;
            _cc = new ObservableCollection<MailContactContext>();
            _to = new ObservableCollection<MailContactContext>();
            _attachments = new ObservableCollection<AttachmentContext>();
            _elevateTrustLevelCommand = new RelayCommand(OnElevateTrustCommand);
        }

        #endregion

        #region Events

        public event EventHandler DownloadCompleted;

        protected virtual void OnDownloadCompleted() {
            var handler = DownloadCompleted;
            if (handler != null) 
                handler(this, EventArgs.Empty);
        }

        #endregion

        #region Properties

        public bool HasExternalsAndSourceIsNotTrusted {
            get { return HasExternals && !IsSourceTrusted; }
        }

        public bool HasExternals {
            get { return _hasExternals; }
            set {
                if (_hasExternals == value) {
                    return;
                }
                _hasExternals = value;
                RaisePropertyChanged(() => HasExternals);
                RaisePropertyChanged(() => HasExternalsAndSourceIsNotTrusted);
            }
        }

        public bool IsLoaded {
            get { return _isLoaded; }
            set {
                if (_isLoaded == value) 
                    return;

                _isLoaded = value;
                RaisePropertyChanged(() => IsLoaded);
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
                RaisePropertyChanged(() => HasExternalsAndSourceIsNotTrusted);
            }
        }

        public IEnumerable<MailContactContext> Cc {
            get { return _cc; }
        }

        public IEnumerable<MailContactContext> To {
            get { return _to; }
        }

        public IEnumerable<MailContactContext> SecondaryTo {
            get { return _to.Skip(1); }
        }

        public IEnumerable<AttachmentContext> Attachments {
            get { return _attachments; }
        }

        public bool HasMultipleRecipients {
            get { return _to.Count > 1; }
        }

        public bool HasAttachments {
            get { return _message.HasAttachments; }
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

        public bool HasCarbonCopies {
            get { return _cc.Count > 0; }
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
            get { return _message.HasFlag(MailMessageFlags.Flagged); }
            set {
                if (_message.HasFlag(MailMessageFlags.Flagged) == value) {
                    return;
                }

                if (value) {
                    _message.WriteFlag(MailMessageFlags.Flagged);
                } else {
                    _message.DropFlag(MailMessageFlags.Flagged);
                }

                RaisePropertyChanged(() => IsFlagged);
                RaisePropertyChanged(() => IsNotFlagged);
                OnFlaggedStatusChanged();
            }
        }

        public bool IsSeen {
            get { return _message.HasFlag(MailMessageFlags.Seen); }
            set {
                if (_message.HasFlag(MailMessageFlags.Seen) == value) {
                    return;
                }

                if (value) {
                    _message.WriteFlag(MailMessageFlags.Seen);
                } else {
                    _message.DropFlag(MailMessageFlags.Seen);
                }

                RaisePropertyChanged(() => IsSeen);
                RaisePropertyChanged(() => IsNotSeen);
                OnSeenStatusChanged();
            }
        }

        public bool IsAnswered {
            get { return _message.HasFlag(MailMessageFlags.Answered); }
            set {
                if (_message.HasFlag(MailMessageFlags.Seen) == value) {
                    return;
                }

                if (value) {
                    _message.WriteFlag(MailMessageFlags.Answered);
                } else {
                    _message.DropFlag(MailMessageFlags.Answered);
                }

                RaisePropertyChanged(() => IsAnswered);
                OnAnsweredStatusChanged();
            }
        }

        #endregion

        private async void OnSeenStatusChanged() {
            await SaveFlagsToDatabaseAsync();
        }
        private async void OnAnsweredStatusChanged() {
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

        public bool IsLoading {
            get { return _load > 0; }
        }

        public bool IsNotLoading {
            get { return _load == 0; }
        }

        public MailContactContext From {
            get { return _from; }
            set {
                if (_from == value) 
                    return;

                _from = value;
                RaisePropertyChanged(() => From);
            }
        }

        public MailContactContext PrimaryTo {
            get { return _to.FirstOrDefault(); }
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

            Progress = Convert.ToInt32(percentage);
        }

        public double Progress {
            get { return _progress; }
            set {
                if (Math.Abs(_progress - value) < double.Epsilon) {
                    return;
                }
                _progress = value;
                RaisePropertyChanged(() => Progress);
            }
        }

        /// <summary>
        /// Downloads the message mime structure from the server.
        /// </summary>
        /// <returns>The mime structure as a byte array.</returns>
        internal async Task<byte[]> DownloadAsync() {
            Application.Current.AssertBackgroundThread();

            try {
                IncrementLoad();

                var mime = await FetchMimeAsync();
                using (var context = new DatabaseContext()) {
                    var message = await context.MailMessages.FindAsync(_message.Id);
                    var mimeMessage = new MimeMessageModel {
                        Data = mime
                    };

                    message.MimeMessages.Add(mimeMessage);
                    var reader = new MailMessageReader(mime);

                    message.HasAttachments = reader.FindAllAttachments().Count > 0;

                    var from = await context.MailContacts
                        .FirstOrDefaultAsync(x => x.Address == reader.Headers.From.Address);
                    if (from == null) {
                        from = new MailContactModel {
                            Name = reader.Headers.From.DisplayName,
                            Address = reader.Headers.From.Address
                        };

                        context.MailContacts.Add(from);
                    }

                    foreach (var value in reader.Headers.To) {
                        var v = value;
                        var contact = await context.MailContacts
                            .FirstOrDefaultAsync(x => x.Address == v.Address);
                        if (contact == null) {
                            contact = new MailContactModel {
                                Name = v.DisplayName,
                                Address = v.Address
                            };

                            context.MailContacts.Add(contact);
                        }
                        _to.Add(new MailContactContext(contact));
                    }

                    foreach (var value in reader.Headers.Cc) {
                        var v = value;
                        var contact = await context.MailContacts
                            .FirstOrDefaultAsync(x => x.Address == v.Address);
                        if (contact == null) {
                            contact = new MailContactModel {
                                Name = v.DisplayName,
                                Address = v.Address
                            };

                            context.MailContacts.Add(contact);
                        }
                        _cc.Add(new MailContactContext(contact));
                    }

                    await context.SaveChangesAsync();
                }

                await Application.Current.Dispatcher
                    .InvokeAsync(OnDownloadCompleted);

                return mime;

            } finally {
                DecrementLoad();
            }
        }

        internal Task<bool> GetIsMimeLoadedAsync() {
            Application.Current.AssertBackgroundThread();

            using (var database = new DatabaseContext()) {
                return database.MimeMessages.Where(x => x.MessageId == Id).AnyAsync();
            }
        }

        private Task<MailboxModel> GetMailboxAsync() {
            Application.Current.AssertBackgroundThread();

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
                var mime = await database.MimeMessages.FirstAsync(x => x.MessageId == Id);
                var reader = new MailMessageReader(mime.Data);

                var part = reader.FindFirstHtmlVersion();
                if (part == null) {
                    return;
                }

                var text = part.GetBodyAsText();
                const string pattern = "(href|src)\\s*=\\s*(\"|&quot;).+?(\"|&quot;)";
                HasExternals = Regex.IsMatch(text, pattern,
                    RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

                if (!HasExternals) {
                    return;
                }

                var contact = await database.MailContacts
                    .FirstOrDefaultAsync(x => x.Address == FromAddress);

                IsSourceTrusted = contact != null && contact.IsTrusted;
            }
        }

        internal void InvalidateBindings() {
            Application.Current.AssertUIThread();

            RaisePropertyChanged(() => HasAttachments);
            RaisePropertyChanged(() => IsSourceTrusted);
        }

        private async void OnElevateTrustCommand(object obj) {
            try {
                await Task.Run(async () => {
                    await TrustSourceAsync();
                });
                await App.Context.RefreshMessageSelectionAsync(this);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Loads all message details from the database.
        /// </summary>
        /// <returns>An awaitable.</returns>
        public async Task LoadAsync() {
            try {
                using (var context = new DatabaseContext()) {
                    var mime = await context.MimeMessages.FirstOrDefaultAsync(x => x.MessageId == _message.Id);
                    var reader = new MailMessageReader(mime.Data);

                    var from = await context.MailContacts
                            .FirstOrDefaultAsync(x => x.Address == reader.Headers.From.Address);
                    From = new MailContactContext(from);

                    foreach (var value in reader.Headers.To) {
                        var v = value;
                        var contact = await context.MailContacts
                            .FirstOrDefaultAsync(x => x.Address == v.Address);

                        _to.Add(new MailContactContext(contact));
                    }

                    foreach (var value in reader.Headers.Cc) {
                        var v = value;
                        var contact = await context.MailContacts
                            .FirstOrDefaultAsync(x => x.Address == v.Address);

                        _cc.Add(new MailContactContext(contact));
                    }

                    foreach (var attachment in reader.FindAllAttachments()) {
                        _attachments.Add(new AttachmentContext(attachment));
                    }

                    RaisePropertyChanged(() => PrimaryTo);
                    RaisePropertyChanged(() => SecondaryTo);
                    RaisePropertyChanged(() => HasCarbonCopies);
                    RaisePropertyChanged(() => HasMultipleRecipients);

                    IsLoaded = true;
                }
            } catch (Exception ex) {
                Logger.Error(ex);
                throw;
            }
        }
    }
}
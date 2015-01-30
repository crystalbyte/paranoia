﻿#region Using directives

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
    public class MailMessageContext : SelectionObject {

        #region Private Fields

        private int _load;
        private long _bytesReceived;
        private int _progressChanged;
        private readonly MailMessageModel _message;
        private readonly ICommand _elevateTrustLevelCommand;
        private readonly MailboxContext _mailbox;

        private readonly ObservableCollection<AttachmentContext> _attachments;
        private bool _isSourceTrusted;
        private bool _hasExternals;

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
                await Task.Run(async () => {
                    await TrustSourceAsync();
                });
                await App.Context.RefreshMessageSelectionAsync(this);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
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

        public bool IsRecycled {
            get { return _message == null; }
        }

        public ICollection<AttachmentContext> Attachments {
            get { return _attachments; }
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
                    _message.HasAttachments = message.HasAttachments;

                    await context.SaveChangesAsync();
                }
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
    }
}
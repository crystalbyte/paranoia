#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia
// 
// Crystalbyte.Paranoia is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License.
// 
// Foobar is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Using Directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Data.SQLite;
using Crystalbyte.Paranoia.Mail;
using NLog;

#endregion

namespace Crystalbyte.Paranoia {
    [DebuggerDisplay("Id = {Id}, Subject = {Subject}")]
    public class MailMessageContext : SelectionObject, IMailMessage, IAuthenticatable, IBlockable {

        #region Private Fields

        private double _progress;
        private bool _hasExternals;
        private bool _isDownloading;
        private bool _isExternalContentAllowed;
        private Authenticity _authenticity;

        private readonly MailMessage _message;
        private readonly MailboxContext _mailbox;
        private readonly MailAddressContext _from;
        private readonly Collection<MailAddressContext> _to;
        private readonly Collection<MailAddressContext> _cc;
        private readonly ObservableCollection<MailAttachmentContext> _attachments;
        private bool _isDetailed;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        internal MailMessageContext(MailboxContext mailbox, MailMessage message) {
            _mailbox = mailbox;
            _attachments = new ObservableCollection<MailAttachmentContext>(
                message.Attachments.Select(x => new MailAttachmentContext(x)));

            _message = message;

            var from = message.Addresses.FirstOrDefault(x => x.Role == AddressRole.From);
            if (from != null) {
                _from = new MailAddressContext(from);
            }

            var cc = message.Addresses.Where(x => x.Role == AddressRole.Cc);
            _cc = new Collection<MailAddressContext>(cc.Select(x => new MailAddressContext(x)).ToArray());

            var to = message.Addresses.Where(x => x.Role == AddressRole.To);
            _to = new Collection<MailAddressContext>(to.Select(x => new MailAddressContext(x)).ToArray());
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

        #region ReadOnly Properties

        public Int64 Size {
            get { return _message.Id; }
        }

        public string Subject {
            get { return _message.Subject; }
        }

        public DateTime Date {
            get { return _message.Date; }
        }

        public long Id {
            get { return _message.Id; }
        }

        public bool HasAttachments {
            get { return _message.Attachments.Count > 0; }
        }

        public long Uid {
            get { return _message.Uid; }
        }

        #endregion

        #region Properties

        public bool IsDetailed {
            get { return _isDetailed; }
            set {
                if (_isDetailed == value) {
                    return;
                }
                _isDetailed = value;
                RaisePropertyChanged(() => IsDetailed);
            }
        }

        public bool HasExternals {
            get { return _hasExternals; }
            set {
                if (_hasExternals == value) {
                    return;
                }
                _hasExternals = value;
                RaisePropertyChanged(() => HasExternals);
            }
        }

        public IReadOnlyCollection<MailAddressContext> Cc {
            get { return _cc; }
        }

        public IReadOnlyCollection<MailAddressContext> To {
            get { return _to; }
        }

        public IReadOnlyCollection<MailAttachmentContext> Attachments {
            get { return _attachments; }
        }

        public bool HasMultipleRecipients {
            get { return _to.Count > 1; }
        }

        public bool HasCarbonCopies {
            get { return _cc.Count > 0; }
        }

        public MailboxContext Mailbox {
            get { return _mailbox; }
        }

        public bool IsSubjectNilOrEmpty {
            get { return Subject == "NIL" || string.IsNullOrEmpty(Subject); }
        }

        public bool IsFlagged {
            get {
                return _message.Flags
                    .Any(x => x.Value.EqualsIgnoreCase(MailMessageFlags.Flagged));
            }
            set {
                if (IsFlagged == value) {
                    return;
                }

                if (value) {
                    _message.Flags.Add(new MailMessageFlag { Value = MailMessageFlags.Flagged });
                } else {
                    _message.Flags.RemoveAll(x => x.Value.EqualsIgnoreCase(MailMessageFlags.Flagged));
                }

                RaisePropertyChanged(() => IsFlagged);
            }
        }

        public bool IsDownloading {
            get { return _isDownloading; }
            set {
                if (_isDownloading == value) {
                    return;
                }

                _isDownloading = value;
                RaisePropertyChanged(() => IsDownloading);
            }
        }

        public bool IsSeen {
            get {
                return _message.Flags
                    .Any(x => x.Value.EqualsIgnoreCase(MailMessageFlags.Seen));
            }
            set {
                if (IsSeen == value) {
                    return;
                }

                if (value) {
                    _message.Flags.Add(new MailMessageFlag { Value = MailMessageFlags.Seen });
                } else {
                    _message.Flags.RemoveAll(x => x.Value.EqualsIgnoreCase(MailMessageFlags.Seen));
                }

                RaisePropertyChanged(() => IsSeen);
            }
        }

        public bool IsAnswered {
            get {
                return _message.Flags
                    .Any(x => x.Value.EqualsIgnoreCase(MailMessageFlags.Answered));
            }
            set {
                if (IsAnswered == value) {
                    return;
                }

                if (value) {
                    _message.Flags.Add(new MailMessageFlag { Value = MailMessageFlags.Answered });
                } else {
                    _message.Flags.RemoveAll(x => x.Value.EqualsIgnoreCase(MailMessageFlags.Answered));
                }

                RaisePropertyChanged(() => IsAnswered);
            }
        }

        public bool IsUnseen {
            get { return !IsSeen; }
        }

        public bool IsUnflagged {
            get { return !IsFlagged; }
        }

        public MailAddressContext From {
            get { return _from; }
        }

        #endregion

        #region Methods

        private async Task SetAuthenticityAsync(Authenticity authenticity) {
            Logger.Enter();

            Application.Current.AssertUIThread();

            var state = Authenticity;
            try {
                Authenticity = authenticity;

                await Task.Run(async () => {
                    using (var context = new DatabaseContext()) {
                        var contact = await context.MailContacts.FirstOrDefaultAsync(x => x.Address == From.Address);
                        if (contact == null) {
                            throw new MissingContactException();
                        }

                        contact.Authenticity = authenticity;
                        await context.SaveChangesAsync(OptimisticConcurrencyStrategy.ClientWins);
                    }
                });

            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
                Authenticity = state;
            } finally {
                Logger.Exit();
            }
        }

        private async Task<byte[]> FetchMimeAsync() {
            Application.Current.AssertBackgroundThread();

            Mailbox mailbox;
            MailAccount account;
            using (var context = new DatabaseContext()) {
                mailbox = context.Mailboxes.Find(_message.MailboxId);
                account = context.MailAccounts.Find(mailbox.AccountId);
            }

            using (var connection = new ImapConnection { Security = account.ImapSecurity }) {
                connection.RemoteCertificateValidationFailed += (sender, e) => e.IsCanceled = false;
                using (var auth = await connection.ConnectAsync(account.ImapHost, account.ImapPort)) {
                    using (var session = await auth.LoginAsync(account.ImapUsername, account.ImapPassword)) {
                        var folder = await session.SelectAsync(mailbox.Name);

                        folder.ByteCountChanged += OnByteCountChanged;
                        var mime = await folder.PeekMessageBodyAsync(Uid);
                        folder.ByteCountChanged -= OnByteCountChanged;

                        return mime;
                    }
                }
            }
        }

        private void OnByteCountChanged(object sender, Mail.ProgressChangedEventArgs e) {
            try {
                var percentage = (Convert.ToDouble(e.ByteCount) / Convert.ToDouble(Size)) * 100;

                // Total bytes and the actual size may differ due to encoding and compression.
                if (percentage > 100) {
                    percentage = 100;
                }

                Progress = Convert.ToInt32(percentage);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
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
        /// Loads detailed information for the message.
        /// This method is invoked after a message is being selected to limit database calls.
        /// </summary>
        /// <returns>The state task.</returns>
        public async Task DetailAsync() {
            Logger.Enter();

            Application.Current.AssertUIThread();

            try {
                var getIsExternalContentAllowed = GetIsExternalContentAllowedAsync();
                var getAuthenticity = GetAuthenticityAsync();

                IsExternalContentAllowed = await getIsExternalContentAllowed;
                Authenticity = await getAuthenticity;

                IsDetailed = true;

            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                Logger.Exit();
            }
        }

        /// <summary>
        ///     Downloads the message mime structure from the server.
        /// </summary>
        /// <returns>The mime structure as a byte array.</returns>
        internal async Task<byte[]> FetchAndDecryptAsync() {
            Logger.Enter();

            Application.Current.AssertUIThread();

            try {
                IsDownloading = true;

                var content = await Task.Run(async () => {
                    var mime = await FetchMimeAsync();
                    var encryption = new EllipticCurveMimeEncryption();
                    mime = encryption.Decrypt(mime);
                    await StoreContentAsync(mime);
                    return mime;
                });

                OnDownloadCompleted();
                return content;
            } finally {
                Logger.Exit();
                IsDownloading = false;
            }
        }

        private async Task StoreContentAsync(byte[] mime) {
            Logger.Enter();

            Application.Current.AssertBackgroundThread();

            try {
                using (var context = new DatabaseContext()) {
                    await context.OpenAsync();

                    MailContact contact = null;
                    if (From != null) {
                        var address = From.Address;
                        contact = await context.MailContacts.FirstAsync(x => x.Address == address);
                    }

                    using (var transaction = context.Database.Connection.BeginTransaction()) {
                        try {
                            var reader = new MailMessageReader(mime);
                            var message = new MailMessage {
                                Id = _message.Id,
                                Mime = mime
                            };

                            context.MailMessages.Attach(message);
                            context.Entry(message).Property(x => x.Mime).IsModified = true;

                            var attachments = reader.FindAllAttachments();
                            message.Attachments.AddRange(attachments.Select(x => new MailAttachment {
                                ContentId = x.ContentId,
                                ContentDisposition = x.ContentDisposition.ToString(),
                                ContentType = x.ContentType.ToString(),
                                Filename = x.FileName,
                                Size = x.Body.Length
                            }));

                            // May be an incomplete draft.
                            if (contact != null) {
                                var xHeaders = reader.Headers.UnknownHeaders;
                                var values = xHeaders.GetValues(MessageHeaders.Signet);
                                if (values != null) {
                                    var value = values.FirstOrDefault();
                                    if (value != null) {
                                        var dic = value.ToKeyValuePairs();
                                        try {
                                            var pKey = Convert.FromBase64String(dic["pkey"]);
                                            var k = contact.Keys.FirstOrDefault(x => x.Bytes == pKey);
                                            if (k == null) {
                                                contact.Keys.Add(new PublicKey {
                                                    Bytes = pKey
                                                });
                                            }
                                        } catch (Exception ex) {
                                            Logger.ErrorException(ex.Message, ex);
                                        }
                                    }
                                }
                            }


                            var textParam = new SQLiteParameter("@text");
                            var idParam = new SQLiteParameter("@message_id", message.Id);
                            var part = reader.FindFirstPlainTextVersion();
                            if (part != null) {
                                textParam.Value = part.GetBodyAsText();
                            } else {
                                part = reader.FindFirstHtmlVersion();
                                if (part != null) {
                                    var document = new CsQuery.CQ(part.GetBodyAsText());
                                    document.Remove("style");
                                    document.Remove("script");
                                    textParam.Value = document.Text().Trim();
                                }
                            }

                            var tableAttribute = typeof(MailMessageContent).GetCustomAttribute<TableAttribute>();
                            var tableName = tableAttribute != null
                                ? tableAttribute.Name
                                : typeof(MailMessageContent).Name;

                            var command = string.Format("INSERT INTO {0}(text, message_id) VALUES(@text, @message_id);", tableName);
                            context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, command, textParam, idParam);

                            await context.SaveChangesAsync(OptimisticConcurrencyStrategy.ClientWins);
                            transaction.Commit();
                        } catch (Exception ex) {
                            transaction.Rollback();
                            Logger.ErrorException(ex.Message, ex);
                            throw;
                        }
                    }
                }
            } finally {
                Logger.Exit();
            }
        }

        internal Task<bool> GetIsMimeStoredAsync() {
            Logger.Enter();

            try {
                var id = _message.Id;
                return Task.Run(() => {
                    using (var database = new DatabaseContext()) {
                        return database.MailMessages.Where(x => x.Id == id).Select(x => x.Mime != null).FirstAsync();
                    }
                });
            } finally {
                Logger.Exit();
            }
        }

        private async Task<Authenticity> GetAuthenticityAsync() {
            Logger.Enter();

            Application.Current.AssertUIThread();

            try {
                if (From == null) {
                    return Authenticity.Confirmed;
                }

                var address = From.Address;
                var auth = await Task.Run(() => {
                    using (var context = new DatabaseContext()) {
                        return
                            context.MailContacts
                                .Where(x => x.Address == address)
                                .Select(x => x.Authenticity)
                                .FirstOrDefaultAsync();
                    }
                });

                return auth;
            } finally {
                Logger.Exit();
            }
        }

        private async Task<bool> GetIsExternalContentAllowedAsync() {
            Logger.Enter();

            Application.Current.AssertUIThread();

            try {
                var addresses = _message.Addresses
                    .Where(x => x.Role == AddressRole.From)
                    .ToArray();

                if (addresses.Length == 0) {
                    return false;
                }

                var first = addresses.First().Address;

                return await Task.Run(() => {
                    using (var context = new DatabaseContext()) {
                        return context.MailContacts
                            .Where(x => x.Address == first)
                            .Select(x => x.IsExternalContentAllowed)
                            .FirstAsync();
                    }
                });
            } finally {
                Logger.Exit();
            }
        }

        #endregion

        #region Implementation of IBlockable

        public event EventHandler IsExternalContentAllowedChanged;

        protected virtual void OnIsExternalContentAllowedChanged() {
            var handler = IsExternalContentAllowedChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public bool IsExternalContentAllowed {
            get { return _isExternalContentAllowed; }
            set {
                if (_isExternalContentAllowed == value) {
                    return;
                }
                _isExternalContentAllowed = value;
                RaisePropertyChanged(() => IsExternalContentAllowed);
                OnIsExternalContentAllowedChanged();
            }
        }

        public Task BlockAsync() {
            Application.Current.AssertUIThread();

            IsExternalContentAllowed = false;
            return SetContentBlockAsync(false);
        }

        private async Task SetContentBlockAsync(bool isContentAllowed) {
            Logger.Enter();

            try {
                var addresses = _message.Addresses
                    .Where(x => x.Role == AddressRole.From)
                    .ToArray();

                if (addresses.Length == 0) {
                    throw new InvalidOperationException("Content rules can't be applied on messages which do not have a sender.");
                }

                var address = addresses.First().Address;

                await Task.Run(async () => {
                    using (var context = new DatabaseContext()) {
                        var contact = await context.Set<MailContact>().FirstAsync(x => x.Address == address);
                        contact.IsExternalContentAllowed = isContentAllowed;
                        await context.SaveChangesAsync(OptimisticConcurrencyStrategy.ClientWins);
                    }
                });
            } finally {
                Logger.Exit();
            }
        }

        public Task UnblockAsync() {
            Application.Current.AssertUIThread();

            IsExternalContentAllowed = true;
            return SetContentBlockAsync(true);
        }

        #endregion

        #region Implementation of IAuthenticatable

        public event EventHandler AuthenticityChanged;

        protected virtual void OnAuthenticityChanged() {
            var handler = AuthenticityChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public Authenticity Authenticity {
            get { return _authenticity; }
            set {
                if (_authenticity == value) {
                    return;
                }
                _authenticity = value;
                RaisePropertyChanged(() => Authenticity);
                OnAuthenticityChanged();
            }
        }

        public Task ConfirmAsync() {
            return SetAuthenticityAsync(Authenticity.Confirmed);
        }

        public Task RejectAsync() {
            return SetAuthenticityAsync(Authenticity.Rejected);
        }

        #endregion
    }
}
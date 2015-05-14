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
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Crystalbyte.Paranoia.Cryptography;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Data.SQLite;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.UI.Commands;
using CsQuery;
using NLog;

#endregion

namespace Crystalbyte.Paranoia {
    [DebuggerDisplay("Subject = {Subject}, Address = {FromAddress}")]
    public class MailMessageContext : SelectionObject, IMailMessage {
        #region Private Fields

        private int _load;
        private long _bytesReceived;
        private double _progress;
        private bool _isExternalContentAllowed;
        private bool _hasExternals;
        private MailContactContext _from;
        private MailMessage _message;

        private readonly MailboxContext _mailbox;
        private readonly ObservableCollection<MailContactContext> _to;
        private readonly ObservableCollection<MailContactContext> _cc;
        private readonly ObservableCollection<AttachmentContext> _attachments;
        private readonly ICommand _allowExternalContentCommand;
        private readonly ICommand _classifyContactCommand;
        private bool _isInitialized;
        private bool _isFishy;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        internal MailMessageContext(MailboxContext mailbox, MailMessage message) {
            _mailbox = mailbox;
            _message = message;
            _cc = new ObservableCollection<MailContactContext>();
            _to = new ObservableCollection<MailContactContext>();
            _attachments = new ObservableCollection<AttachmentContext>();
            _allowExternalContentCommand = new RelayCommand(OnAllowExternal);
            _classifyContactCommand = new RelayCommand(OnClassifyContact);
        }

        #endregion

        #region Events

        public event EventHandler Initialized;

        protected virtual void OnInitialized() {
            var handler = Initialized;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public event EventHandler AllowExternalContentChanged;

        protected virtual void OnAllowExternalContentChanged() {
            var handler = AllowExternalContentChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public event EventHandler DownloadCompleted;

        protected virtual void OnDownloadCompleted() {
            var handler = DownloadCompleted;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion

        #region Properties

        public bool IsFishy {
            get { return _isFishy; }
            set {
                if (_isFishy == value) {
                    return;
                }

                _isFishy = value;
                RaisePropertyChanged(() => IsFishy);
            }
        }

        public bool HasExternalsAndSourceIsNotTrusted {
            get { return HasExternals && !IsExternalContentAllowed; }
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

        public bool IsInitialized {
            get { return _isInitialized; }
            set {
                if (_isInitialized == value)
                    return;

                _isInitialized = value;
                RaisePropertyChanged(() => IsInitialized);
            }
        }

        public bool IsExternalContentAllowed {
            get { return _isExternalContentAllowed; }
            set {
                if (_isExternalContentAllowed == value) {
                    return;
                }
                _isExternalContentAllowed = value;
                RaisePropertyChanged(() => IsExternalContentAllowed);
                RaisePropertyChanged(() => HasExternalsAndSourceIsNotTrusted);
                OnAllowExternalContentChanged();
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

        public ICommand AllowExternalContentCommand {
            get { return _allowExternalContentCommand; }
        }

        public ICommand ClassifyContactCommand {
            get { return _classifyContactCommand; }
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
                if (value) {
                    _message.Flags.Add(new MessageFlag { Value = MailMessageFlags.Flagged });
                } else {
                    _message.Flags.RemoveAll(x => x.Value.EqualsIgnoreCase(MailMessageFlags.Flagged));
                }
            }
        }

        public bool IsSeen {
            get {
                return _message.Flags
                    .Any(x => x.Value.EqualsIgnoreCase(MailMessageFlags.Seen));
            }
            set {
                if (value) {
                    _message.Flags.Add(new MessageFlag { Value = MailMessageFlags.Seen });
                } else {
                    _message.Flags.RemoveAll(x => x.Value.EqualsIgnoreCase(MailMessageFlags.Seen));
                }
            }
        }

        public bool IsAnswered {
            get {
                return _message.Flags
                    .Any(x => x.Value.EqualsIgnoreCase(MailMessageFlags.Answered));
            }
            set {
                if (value) {
                    _message.Flags.Add(new MessageFlag { Value = MailMessageFlags.Answered });
                } else {
                    _message.Flags.RemoveAll(x => x.Value.EqualsIgnoreCase(MailMessageFlags.Answered));
                }
            }
        }

        #endregion

        private void OnClassifyContact(object obj) {
            var cc = (ContactClassification)obj;

            if (cc == ContactClassification.Genuine
                || cc == ContactClassification.Spam) {
                IsFishy = false;
            }

            throw new NotImplementedException();
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
                        var mime = await folder.FetchMessageBodyAsync(Uid);
                        folder.ByteCountChanged -= OnByteCountChanged;

                        return mime;
                    }
                }
            }
        }

        private void OnByteCountChanged(object sender, ProgressChangedEventArgs e) {
            try {
                BytesReceived = e.ByteCount;
                var percentage = (Convert.ToDouble(e.ByteCount) / Convert.ToDouble(Size)) * 100;

                // Total bytes and the actual size may differ due to encoding and compression.
                if (percentage > 100) {
                    percentage = 100;
                }

                Progress = Convert.ToInt32(percentage);
            } catch (Exception ex) {
                Logger.Error(ex);
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
        ///     Downloads the message mime structure from the server.
        /// </summary>
        /// <returns>The mime structure as a byte array.</returns>
        internal async Task<byte[]> FetchAndDecryptAsync() {
            Application.Current.AssertBackgroundThread();

            try {
                IncrementLoad();

                var mime = await FetchMimeAsync();
                var reader = new MailMessageReader(mime);

                var address = FromAddress;
                var parts = reader.FindAllMessagePartsWithMediaType(MediaTypes.EncryptedMime);
                var xHeaders = reader.Headers.UnknownHeaders;

                byte[] pKey = null;
                byte[] nonce = null;
                var n = (xHeaders.GetValues(MessageHeaders.Nonce) ?? new[] { string.Empty }).FirstOrDefault();
                if (!string.IsNullOrEmpty(n)) {
                    nonce = Convert.FromBase64String(n);
                }

                for (var i = 0; i < xHeaders.Count; i++) {
                    var key = xHeaders.GetKey(i);
                    if (!key.EqualsIgnoreCase(MessageHeaders.Signet))
                        continue;

                    var values = xHeaders.GetValues(i);
                    if (values == null) {
                        throw new SignetMissingOrCorruptException(address);
                    }

                    var signet = values.First();
                    var split = signet.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    var p = split[0].Substring(split[0].IndexOf('=') + 1).Trim(';');
                    pKey = Convert.FromBase64String(p);
                }

                if (parts != null && parts.Count > 0) {
                    var part = parts.First();

                    using (var context = new DatabaseContext()) {
                        var current = context.KeyPairs.OrderByDescending(x => x.Date).First();
                        var contact = context.MailContacts
                            .Include(x => x.Keys)
                            .FirstOrDefault(x => x.Address == address);

                        if (contact == null) {
                            throw new MissingContactException(address);
                        }

                        if (contact.Keys.Count == 0) {
                            throw new MissingKeyException(address);
                        }

                        var crypto = new PublicKeyCrypto(current.PublicKey, current.PrivateKey);
                        mime = crypto.DecryptWithPrivateKey(part.Body, pKey, nonce);
                    }
                }

                await StoreContentAsync(mime);

                return mime;
            } finally {
                DecrementLoad();
            }
        }

        private async Task StoreContentAsync(byte[] mime) {
            Application.Current.AssertBackgroundThread();

            using (var context = new DatabaseContext()) {
                context.Connect();
                context.EnableForeignKeys();

                using (var transaction = context.Database.Connection.BeginTransaction()) {
                    try {
                        _message = context.MailMessages.Find(_message.Id);

                        var reader = new MailMessageReader(mime);
                        _message.HasAttachments = reader.FindAllAttachments().Count > 0;

                        var data = new MailData {
                            Mime = mime
                        };

                        _message.Data.Add(data);

                        var name = FromName;
                        var address = FromAddress;
                        var from = context.MailContacts
                            .FirstOrDefault(x => x.Address == address);

                        if (from == null) {
                            from = new MailContact {
                                Name = name,
                                Address = address
                            };

                            context.MailContacts.Add(from);
                        }

                        var xHeaders = reader.Headers.UnknownHeaders;
                        var values = xHeaders.GetValues(MessageHeaders.Signet);
                        if (values != null) {
                            var value = values.FirstOrDefault();
                            if (value != null) {
                                var dic = value.ToKeyValuePairs();
                                try {
                                    var pKey = Convert.FromBase64String(dic["pkey"]);
                                    var k = from.Keys.FirstOrDefault(x => x.Data == pKey);
                                    if (k == null) {
                                        from.Keys.Add(new PublicKey {
                                            Data = pKey
                                        });
                                    }
                                } catch (Exception ex) {
                                    Logger.Error(ex);
                                }
                            }
                        }

                        foreach (var value in reader.Headers.To) {
                            var v = value;
                            var contact = context.MailContacts
                                .FirstOrDefault(x => x.Address == v.Address);

                            if (contact == null) {
                                contact = new MailContact {
                                    Name = v.DisplayName,
                                    Address = v.Address
                                };

                                context.MailContacts.Add(contact);
                            }
                            _to.Add(new MailContactContext(contact));
                        }

                        foreach (var value in reader.Headers.Cc) {
                            var v = value;
                            var contact = context.MailContacts
                                .FirstOrDefault(x => x.Address == v.Address);

                            if (contact == null) {
                                contact = new MailContact {
                                    Name = v.DisplayName,
                                    Address = v.Address
                                };

                                context.MailContacts.Add(contact);
                            }
                            _cc.Add(new MailContactContext(contact));
                        }

                        await context.SaveChangesAsync(OptimisticConcurrencyStrategy.ClientWins);

                        var text = new SQLiteParameter("@text");
                        var id = new SQLiteParameter("@message_id", _message.Id);
                        var part = reader.FindFirstPlainTextVersion();
                        if (part != null) {
                            text.Value = part.GetBodyAsText();
                        } else {
                            part = reader.FindFirstHtmlVersion();
                            if (part != null) {
                                var document = new CQ(part.GetBodyAsText());
                                document.Remove("style");
                                document.Remove("script");
                                text.Value = document.Text().Trim();
                            }
                        }

                        // TODO: Try and generate SQL statement from entity attributes.
                        const string command = "INSERT INTO mail_content(text, message_id) VALUES(@text, @message_id);";
                        context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, command, text, id);
                        transaction.Commit();

                    } catch (Exception ex) {
                        transaction.Rollback();
                        Logger.Error(ex);
                        throw;
                    }
                }
            }
        }

        internal Task<bool> GetIsMimeStoredAsync() {
            return Task.Run(() => {
                using (var database = new DatabaseContext()) {
                    return database.MailData.AnyAsync(x => x.MessageId == Id);
                }
            });
        }

        internal async Task AllowExternalContentAsync() {
            using (var database = new DatabaseContext()) {
                var contact = await database.MailContacts
                    .FirstAsync(x => x.Address == FromAddress);

                contact.IsExternalContentAllowed = true;
                await database.SaveChangesAsync();

                await Application.Current.Dispatcher
                    .InvokeAsync(() => { IsExternalContentAllowed = true; });
            }
        }

        private async void OnAllowExternal(object obj) {
            await Task.Run(async () => { await AllowExternalContentAsync(); });
            await App.Context.ViewMessageAsync(this);
        }

        /// <summary>
        ///     Loads all message details from the database.
        /// </summary>
        /// <returns>An awaitable.</returns>
        public async Task InitDetailsAsync() {
            Application.Current.AssertUIThread();

            Logger.Info("BEGIN InitDetailsAsync");

            if (IsInitialized) {
                Logger.Warn("Method InitDetailsAsync() called while already loaded ...");
            }

            var address = FromAddress;
            await Task.Run(async () => {
                using (var context = new DatabaseContext()) {
                    var content = await context.MailData.FirstOrDefaultAsync(x => x.MessageId == _message.Id);
                    var reader = new MailMessageReader(content.Mime);

                    var from = await context.MailContacts
                        .FirstOrDefaultAsync(x => x.Address == address);
                    await Application.Current.Dispatcher.InvokeAsync(() => {
                        From = new MailContactContext(from);
                        IsExternalContentAllowed = from.IsExternalContentAllowed;
                    });

                    var part = reader.FindFirstHtmlVersion();
                    if (part == null) {
                        return;
                    }

                    var text = part.GetBodyAsText();
                    const string pattern = "(href|src)\\s*=\\s*(\"|&quot;)http.+?(\"|&quot;)";
                    var externals = Regex.IsMatch(text, pattern,
                        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    await Application.Current.Dispatcher.InvokeAsync(() => { HasExternals = externals; });

                    if (from.Classification == ContactClassification.Default) {
                        var analyzer = new SimpleSpamDetector(text);
                        var isfishy = await analyzer.GetIsSpamAsync();
                        await Application.Current.Dispatcher.InvokeAsync(
                            () => { IsFishy = isfishy; });
                    }

                    var to = new List<MailContactContext>();
                    foreach (var value in reader.Headers.To) {
                        var v = value;
                        var contact = await context.MailContacts
                            .FirstOrDefaultAsync(x => x.Address == v.Address);

                        to.Add(new MailContactContext(contact));
                    }

                    await Application.Current.Dispatcher.InvokeAsync(() => _to.AddRange(to));

                    var cc = new List<MailContactContext>();
                    foreach (var value in reader.Headers.Cc) {
                        var v = value;
                        var contact = await context.MailContacts
                            .FirstOrDefaultAsync(x => x.Address == v.Address);

                        cc.Add(new MailContactContext(contact));
                    }

                    await Application.Current.Dispatcher.InvokeAsync(() => cc.AddRange(to));

                    var attachments = reader.FindAllAttachments()
                        .Select(attachment => new AttachmentContext(attachment)).ToList();

                    await Application.Current.Dispatcher.InvokeAsync(
                            () => _attachments.AddRange(attachments));

                    await Application.Current.Dispatcher.InvokeAsync(() => {
                        RaisePropertyChanged(() => PrimaryTo);
                        RaisePropertyChanged(() => SecondaryTo);
                        RaisePropertyChanged(() => HasAttachments);
                        RaisePropertyChanged(() => HasCarbonCopies);
                        RaisePropertyChanged(() => HasMultipleRecipients);

                        IsInitialized = true;
                        OnInitialized();
                    });
                }

                Logger.Info("END InitDetailsAsync");
            });
        }
    }
}
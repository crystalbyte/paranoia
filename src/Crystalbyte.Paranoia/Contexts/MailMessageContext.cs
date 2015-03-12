#region Using directives

using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
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
    public class MailMessageContext : SelectionObject, IMailMessage {

        #region Private Fields

        private int _load;
        private long _bytesReceived;
        private double _progress;
        private bool _isExternalContentAllowed;
        private bool _hasExternals;
        private MailContactContext _from;
        private MailMessageModel _message;

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

        internal MailMessageContext(MailboxContext mailbox, MailMessageModel message) {
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

        private async void OnClassifyContact(object obj) {
            var cc = (ContactClassification)obj;

            if (cc == ContactClassification.Genuine
                || cc == ContactClassification.Spam) {
                IsFishy = false;
            }

            await ChangeClassificationAsync(cc);
        }

        private async Task ChangeClassificationAsync(ContactClassification classification) {
            _from.Classification = classification;
            await _from.SaveAsync();
        }

        private Task SaveFlagsToDatabaseAsync() {
            var flags = _message.Flags;
            return Task.Run(() => {
                try {
                    using (var context = new DatabaseContext()) {
                        var m = context.MailMessages.Find(_message.Id);
                        m.Flags = flags;

                        // Handle Optimistic Concurrency.
                        // https://msdn.microsoft.com/en-us/data/jj592904.aspx?f=255&MSPPError=-2147217396
                        while (true) {
                            try {
                                context.SaveChanges();
                                break;
                            } catch (DbUpdateConcurrencyException ex) {
                                ex.Entries.ForEach(x => x.Reload());
                                Logger.Info(ex);
                            }
                        }

                        _message = m;
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                }
            });
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
            Application.Current.AssertBackgroundThread();

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

                    _message = message;
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
            await Task.Run(async () => {
                await AllowExternalContentAsync();
            });
            await App.Context.ViewMessageAsync(this);
        }

        /// <summary>
        /// Loads all message details from the database.
        /// </summary>
        /// <returns>An awaitable.</returns>
        public async Task InitDetailsAsync() {
            Application.Current.AssertUIThread();

            Logger.Info("BEGIN InitDetailsAsync");

            if (IsInitialized) {
                Logger.Warn("Method InitDetailsAsync() called while already loaded ...");
            }

            await Task.Run(async () => {
                using (var context = new DatabaseContext()) {
                    var mime = await context.MimeMessages.FirstOrDefaultAsync(x => x.MessageId == _message.Id);
                    var reader = new MailMessageReader(mime.Data);

                    var from = await context.MailContacts
                            .FirstOrDefaultAsync(x => x.Address == reader.Headers.From.Address);
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
                    await Application.Current.Dispatcher.InvokeAsync(() => {
                        HasExternals = externals;
                    });

                    if (from.Classification == ContactClassification.Default) {
                        var analyzer = new SimpleSpamDetector(text);
                        var isfishy = await analyzer.GetIsSpamAsync();
                        await Application.Current.Dispatcher.InvokeAsync(() => {
                            IsFishy = isfishy;
                        });
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

                    await Application.Current.Dispatcher.InvokeAsync(() => _attachments.AddRange(attachments));

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
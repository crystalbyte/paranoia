using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Messaging;
using Crystalbyte.Paranoia.Models;
using Crystalbyte.Paranoia.Data;
using NLog;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Crystalbyte.Paranoia.Commands;
using System.Windows.Data;
using System.ComponentModel;

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class MailboxContext : NotificationObject {

        #region Private Fields

        private bool _isSelected;
        private Mailbox _mailbox;
        private ImapMailbox _inbox;
        private string _sortProperty;
        private bool _isSortedAscending;
        private IEnumerable<MailboxFlag> _flags;
        private readonly ImapAccountContext _account;
        private readonly ObservableCollection<MailContext> _mails;
        private readonly CollectionViewSource _mailsViewSource;

        #endregion

        #region Log Declaration

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

        public MailboxContext(ImapAccountContext account, Mailbox mailbox) {
            _account = account;
            _mailbox = mailbox;
            _sortProperty = "Date";
            _mails = new ObservableCollection<MailContext>();
            _mailsViewSource = new CollectionViewSource { Source = _mails };
            _mailsViewSource.Filter += OnMailsViewSourceFilter;
        }

        private void OnMailsViewSourceFilter(object sender, FilterEventArgs e) {
            var mail = e.Item as MailContext;
            if (mail == null) {
                e.Accepted = false;
            }

            e.Accepted = !mail.IsInvite;
        }

        public async Task RestoreAsync(StorageContext context = null) {
            var dispose = false;
            if (context == null) {
                context = new StorageContext();
                context.Mailboxes.Attach(_mailbox);
                dispose = true;
            }

            
            await RestoreFlagsAsync();

            if (dispose) {
                context.Dispose();
            }

            RaisePropertyChanged(string.Empty);
        }

        private Task RestoreFlagsAsync() {
            return Task.Factory.StartNew(() => { _flags = _mailbox.Flags; });
            
        }

        public ImapAccountContext ImapAccount {
            get { return _account; }
        }

        public string Name {
            get { return _mailbox.Name; }
        }

        public bool IsInbox {
            get { return _mailbox.Name.ToLower() == "inbox"; }
        }

        public bool IsAll {
            get { return _mailbox.Flags.Any(x => x.Name.ToLower() == @"\all"); }
        }

        public bool IsTrash {
            get { return _mailbox.Flags.Any(x => x.Name.ToLower() == @"\trash"); }
        }

        public bool IsSent {
            get { return _mailbox.Flags.Any(x => x.Name.ToLower() == @"\sent"); }
        }

        public bool IsFlagged {
            get { return _mailbox.Flags.Any(x => x.Name.ToLower() == @"\flagged"); }
        }

        public bool IsJunk {
            get { return _mailbox.Flags.Any(x => x.Name.ToLower() == @"\junk"); }
        }

        public bool IsImportant {
            get { return _mailbox.Flags.Any(x => x.Name.ToLower() == @"\important"); }
        }

        public bool IsDraft {
            get { return _mailbox.Flags.Any(x => x.Name.ToLower() == @"\drafts"); }
        }

        public bool IsSortedAscending {
            get { return _isSortedAscending; }
            set {
                if (_isSortedAscending == value) {
                    return;
                }

                RaisePropertyChanging(() => IsSortedAscending);
                _isSortedAscending = value;
                RaisePropertyChanged(() => IsSortedAscending);
                Sort();
            }
        }

        public ObservableCollection<MailContext> Mails {
            get { return _mails; }
        }

        public ICollectionView MailsView { 
            get { return _mailsViewSource.View; }
        }

        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected == value) {
                    return;
                }

                RaisePropertyChanging(() => IsSelected);
                _isSelected = value;
                RaisePropertyChanged(() => IsSelected);

                if (value) {
                    OnSelected();
                }
            }
        }

        private async void OnSelected() {
            await RestoreMailsAsync();
            await SyncMailboxAsync();
        }

        public string SortProperty {
            get { return _sortProperty; }
            set {
                if (_sortProperty == value) {
                    _sortProperty = value;
                }

                RaisePropertyChanging(() => SortProperty);
                _sortProperty = value;
                RaisePropertyChanging(() => SortProperty);
                Sort();
            }
        }

        private void Sort() {
            _mailsViewSource.SortDescriptions.Clear();
            if (IsSortedAscending) {
                _mailsViewSource.SortDescriptions.Add(new SortDescription(SortProperty, ListSortDirection.Ascending));
            }
            else {
                _mailsViewSource.SortDescriptions.Add(new SortDescription(SortProperty, ListSortDirection.Descending));
            }
        }

        private async Task RestoreMailsAsync() {
            IEnumerable<Mail> mails = null;
            await Task.Factory.StartNew(() => {
                try {
                    using (var context = new StorageContext()) {
                        var mailbox = context.Mailboxes.Find(_mailbox.Id);
                        mails = mailbox.Mails.ToArray();
                    }
                }
                catch (Exception ex) {
                    Log.Error(ex.Message);
                }
            });

            if (mails != null) {
                _mails.Clear();
                _mails.AddRange(mails.Select(x => new MailContext(this, x)));
                foreach (var mail in _mails) {
                    await mail.RestoreAsync();
                }
            }

            if (_mails.Count > 0) {
                _mails[0].IsSelected = true;
            }
        }

        private async Task SyncMailboxAsync() {
            // Get highest stored uid.
            var max = await Task.Factory.StartNew(() => {
                try {
                    using (var context = new StorageContext()) {
                        var mailbox = context.Mailboxes.Find(_mailbox.Id);
                        return mailbox.Mails.Max(x => x.Uid);
                    }
                }
                catch (Exception ex) {
                    Log.Error(ex.Message);
                }

                return 1;
            });

            using (var connection = new ImapConnection { Security = _account.Security }) {
                using (var authenticator = await connection.ConnectAsync(_account.Host, _account.Port)) {
                    using (var session = await authenticator.LoginAsync(_account.Username, _account.Password)) {
                        var mailbox = await session.SelectAsync(Name);
                        await UpdateMailboxAsync(mailbox);

                        var criteria = string.Format("{0}:* HEADER \"{1}\" \"\"", max, MailHeaders.Type);
                        var uids = await mailbox.SearchAsync(criteria);
                        if (uids.Count == 0) {
                            return;
                        }
                        var envelopes = await mailbox.FetchEnvelopesAsync(uids);

                        foreach (var envelope in envelopes.Where(envelope => envelope.Uid != max).AsParallel()) {
                            await StoreEnvelopeAsync(envelope);
                        }
                    }
                }
            }
        }

        private async Task StoreEnvelopeAsync(ImapEnvelope envelope) {
            try {
                using (var context = new StorageContext()) {
                    var mailbox = await context.Mailboxes.FindAsync(_mailbox.Id);
                    if (mailbox == null) {
                        var message = string.Format("Mailbox with id {0} missing.", _mailbox.Id);
                        throw new InvalidOperationException(message);
                    }

                    var mail = new Mail {
                        Subject = envelope.Subject,
                        InternalDate = envelope.InternalDate,
                        Size = envelope.Size,
                        Uid = envelope.Uid,
                        MessageId = envelope.MessageId,
                        MailContacts = new List<MailContact>(),
                        MailFlags = new List<MailFlag>()
                    };

                    mail.MailFlags.AddRange(envelope.Flags.Select(x => new MailFlag { Name = x }));

                    mail.MailContacts.AddRange(envelope.Sender.Select(x => new MailContact {
                        Type = MailContactType.Sender,
                        Address = x.Address,
                        Name = x.DisplayName,
                    }));
                    mail.MailContacts.AddRange(envelope.From.Select(x => new MailContact {
                        Type = MailContactType.From,
                        Address = x.Address,
                        Name = x.DisplayName
                    }));
                    mail.MailContacts.AddRange(envelope.To.Select(x => new MailContact {
                        Type = MailContactType.To,
                        Address = x.Address,
                        Name = x.DisplayName
                    }));
                    mail.MailContacts.AddRange(envelope.Cc.Select(x => new MailContact {
                        Type = MailContactType.Cc,
                        Address = x.Address,
                        Name = x.DisplayName
                    }));
                    mail.MailContacts.AddRange(envelope.Bcc.Select(x => new MailContact {
                        Type = MailContactType.Bcc,
                        Address = x.Address,
                        Name = x.DisplayName
                    }));

                    mailbox.Mails.Add(mail);
                    context.SaveChanges();

                    var mailContext = new MailContext(this, mail);
                    Mails.Add(mailContext);
                }
            }
            catch (Exception ex) {
                Log.Error(ex.Message);
            }
        }

        private async Task UpdateMailboxAsync(ImapMailbox imapMailbox) {
            await Task.Factory.StartNew(() => {
                try {
                    using (var context = new StorageContext()) {
                        _mailbox = context.Mailboxes.First(x => x.Id == _mailbox.Id);
                        _mailbox.UidNext = imapMailbox.UidNext;
                        _mailbox.Recent = imapMailbox.Recent;
                        _mailbox.UidValidity = imapMailbox.UidValidity;
                        _mailbox.Exists = imapMailbox.Exists;
                        context.SaveChanges();
                    }
                }
                catch (Exception ex) {
                    Log.Error(ex);
                }
            });

            RaisePropertyChanged(string.Empty);
        }

        //public async Task TakeOnlineAsync() {
        //    await ListenAsync();
        //    await SyncAsync();
        //}

        //public async Task TakeOfflineAsync() {
        //    if (_inbox.IsIdle) {
        //        await _inbox.StopIdleAsync();
        //    }
        //}

        //public async Task SyncAsync() {
        //    using (var connection = new ImapConnection { Security = _account.Security }) {
        //        using (var authenticator = await connection.ConnectAsync(_account.Host, _account.ImapPort)) {
        //            using (var session = await authenticator.LoginAsync(_account.ImapUsername, _account.Password)) {
        //                var inbox = await session.SelectAsync(MailboxNames.Inbox);
        //                //var messages = inbox.FetchEnvelopesAsync(new[] { });

        //            }
        //        }
        //    }
        //}

        //public async Task ListenAsync() {
        //    try {
        //        var connection = new ImapConnection { Security = _account.Security };
        //        var authenticator = await connection.ConnectAsync(_account.Host, _account.ImapPort);
        //        var session = await authenticator.LoginAsync(_account.ImapUsername, _account.Password);

        //        _inbox = await session.SelectAsync(MailboxNames.Inbox);
        //        _inbox.MessageReceived += OnMessageReceived;
        //        _inbox.Idle();
        //    }
        //    catch (Exception ex) {
        //        LogContext.Current.PushError(ex);
        //    }
        //}
    }
}

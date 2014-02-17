using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Messaging;
using Crystalbyte.Paranoia.Models;

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class MailboxContext : NotificationObject {

        #region Private Fields

        private bool _isSelected;
        private ImapMailbox _inbox;
        private readonly Mailbox _mailbox;
        private readonly ImapAccountContext _account;
        private readonly IEnumerable<MailboxFlag> _flags;

        #endregion

        public MailboxContext(ImapAccountContext account, Mailbox mailbox) {
            _account = account;
            _mailbox = mailbox;

            // Trigger lazy loading for Flags 
            _flags = _mailbox.Flags;
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

        public bool IsSelected { 
            get { return _isSelected; }
            set {
                if (_isSelected == value) {
                    return;
                }

                RaisePropertyChanging(() => IsSelected);
                _isSelected = value;
                RaisePropertyChanged(() => IsSelected);
            } 
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

        private async void OnMessageReceived(object sender, EventArgs e) {
            //await SyncAsync();
        }

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Messaging;
using Crystalbyte.Paranoia.Models;

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class ImapMailboxContext : NotificationObject {

        private ImapMailbox _inbox;
        private readonly Mailbox _mailbox;
        private readonly ImapAccountContext _account;

        public ImapMailboxContext(ImapAccountContext account, Mailbox mailbox) {
            _account = account;
            _mailbox = mailbox;
        }

        public async Task TakeOnlineAsync() {
            await ListenAsync();
            await SyncAsync();
        }

        public async Task TakeOfflineAsync() {
            if (_inbox.IsIdle) {
                await _inbox.StopIdleAsync();
            }
        }

        public async Task SyncAsync() {
            using (var connection = new ImapConnection { Security = _account.ImapSecurity }) {
                using (var authenticator = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                    using (var session = await authenticator.LoginAsync(_account.ImapUsername, _account.ImapPassword)) {
                        var inbox = await session.SelectAsync(MailboxNames.Inbox);
                        //var messages = inbox.FetchEnvelopesAsync(new[] { });

                    }
                }
            }
        }

        private async void OnMessageReceived(object sender, EventArgs e) {
            await SyncAsync();
        }

        public async Task ListenAsync() {
            try {
                var connection = new ImapConnection { Security = _account.ImapSecurity };
                var authenticator = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort);
                var session = await authenticator.LoginAsync(_account.ImapUsername, _account.ImapPassword);

                _inbox = await session.SelectAsync(MailboxNames.Inbox);
                _inbox.MessageReceived += OnMessageReceived;
                _inbox.Idle();
            }
            catch (Exception ex) {
                ErrorLogContext.Current.PushError(ex);
            }
        }
    }
}

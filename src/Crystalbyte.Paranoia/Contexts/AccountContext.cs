using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Messaging;

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class AccountContext {
        private readonly ImapConnection _connection;

        public AccountContext() {
            _connection = new ImapConnection { Security = SecurityPolicies.Implicit };
            Messages = new ObservableCollection<MessageContext>();
        }

        public async Task SyncAsync() {
            var authenticator = await _connection.ConnectAsync(Host, Port);
            var session = await authenticator.LoginAsync(Username, Password);
            var inbox = await session.SelectAsync("INBOX");
            var uids = await inbox.SearchAsync("ALL");
            var envelopes = await inbox.FetchEnvelopesAsync(uids);
        }

        public ObservableCollection<MessageContext> Messages { get; set; }

        public void Disconnect() {
            _connection.Disconnect();
        }

        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public short Port { get; set; }
    }
}

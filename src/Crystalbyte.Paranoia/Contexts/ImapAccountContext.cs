using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Messaging;

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class ImapAccountContext {
        private readonly ImapConnection _connection;

        public ImapAccountContext() {
            _connection = new ImapConnection { Security = SecurityPolicies.Implicit };
            Messages = new ObservableCollection<MessageContext>();
        }

        public async Task SyncAsync() {
            var authenticator = await _connection.ConnectAsync(Host, Port);
            await authenticator.LoginAsync(Username, Password);
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

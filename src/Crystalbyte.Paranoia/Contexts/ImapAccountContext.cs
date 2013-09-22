using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ActiveUp.Net.Mail;

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class ImapAccountContext {
        private Imap4Client _client;

        public ImapAccountContext() {
            Messages = new ObservableCollection<MessageContext>();
        }

        public Task ConnectAsync() {
            _client = new Imap4Client();

            return Task.Factory.StartNew(() => {
                if (SecurityProtocol == SecurityProtocol.Ssl3) {
                    _client.ConnectSsl(Host, Port);
                } else {
                    _client.Connect(Host, Port);
                    var startTls = _client.ServerCapabilities.Contains("STARTTLS");
                    if (startTls) {
                        
                    }
                }
            });
        }

        public Task LoginAsync(string username, string password) {
            return Task.Factory.StartNew(() => {
                _client.LoginFast(username, password);
            });
        }

        public Task SyncAsync() {
            return Task.Factory.StartNew(() => {
                var mailbox = _client.SelectMailbox("INBOX");
                var uids = mailbox.Search("ALL");
                Messages.AddRange(uids.Select(x => new MessageContext(mailbox.Fetch.BodyStructure(x))));
            });
        }

        public ObservableCollection<MessageContext> Messages { get; set; }

        public void Disconnect() {
            _client.Disconnect();
        }

        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public short Port { get; set; }
        public SecurityProtocol SecurityProtocol { get; set; }
    }
}

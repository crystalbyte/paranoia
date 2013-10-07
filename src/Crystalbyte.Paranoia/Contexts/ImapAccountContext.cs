using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Contexts.Factories;
using Crystalbyte.Paranoia.Messaging;
using Crystalbyte.Paranoia.Models;

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class ImapAccountContext {
        private readonly ImapAccount _account;

        public ImapAccountContext(ImapAccount account) {
            _account = account;
            Messages = new ObservableCollection<ImapEnvelopeContext>();
        }

        public string Host {
            get { return _account.Host; }
        }

        public int Port {
            get { return _account.Port; }
        }

        public string Username {
            get { return _account.Username; }
        }

        public string Password {
            get { return _account.Password; }
        }

        public ImapEnvelopeContextFactory MessageContextFactory { get; internal set; }

        public async Task SyncAsync() {
            using (var connection = new ImapConnection { Security = SecurityPolicies.Implicit }) {
                using (var authenticator = await connection.ConnectAsync(Host, Port)) {
                    using (var session = await authenticator.LoginAsync(Username, Password)) {
                        var inbox = await session.SelectAsync("INBOX");

                        var uids = await inbox.SearchAsync("ALL");
                        if (uids.Count > 0) {
                            var envelopes = await inbox.FetchEnvelopesAsync(uids);
                            Messages.AddRange(envelopes.Select(x => MessageContextFactory.Create(x, this)));
                        }
                    }
                }
            }
        }

        public ObservableCollection<ImapEnvelopeContext> Messages { get; set; }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Messaging;

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class MessageContext {
        private readonly Envelope _envelope;
        private readonly AccountContext _account;

        public MessageContext(Envelope envelope, AccountContext account) {
            _account = account;
            _envelope = envelope;
        }

        public DateTime? Date {
            get { return _envelope.InternalDate; }
        }

        public string Subject {
            get { return _envelope.Subject; }
        }

        public bool IsSeen {
            get { return _envelope.Flags.Any(x => x.ContainsIgnoreCase("\\Seen")); }
        }

        public async Task<string> FetchMessageBodyAsync() {
            using (var connection = new ImapConnection() { Security = SecurityPolicies.Implicit}) {
                using (var authenticator = await connection.ConnectAsync(_account.Host, _account.Port)) {
                    using (var session = await authenticator.LoginAsync(_account.Username, _account.Password)) {
                        var mailbox = await session.SelectAsync("INBOX");
                        return await mailbox.FetchMessageBodyAsync(_envelope.Uid);
                    }
                }
            }
        }
    }
}

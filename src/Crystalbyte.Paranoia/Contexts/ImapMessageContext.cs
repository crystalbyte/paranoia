#region Using directives

using System;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Messaging;
using Crystalbyte.Paranoia.Models;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class ImapMessageContext {
        private readonly ImapEnvelope _envelope;
        private readonly ImapAccountContext _account;
        private readonly string _mailbox;

        public ImapMessageContext(ImapAccountContext account, ImapEnvelope envelope, string mailbox) {
            _envelope = envelope;
            _account = account;
            _mailbox = mailbox;
        }

        public DateTime? Date {
            get { return _envelope.InternalDate; }
        }

        public string Subject {
            get { return _envelope.Subject; }
        }

        /// <summary>
        ///   This key identifies the message across servers, users and mailboxes.
        /// </summary>
        public string Key {
            get { return string.Format("{0}@{1}:{2}", _account.Username, _account.Host, _mailbox); }
        }

        public ImapAccountContext Account {
            get { return _account; }
        }

        public string Mailbox {
            get { return _mailbox; }
        }

        public long Uid {
            get { return _envelope.Uid; }
        }

        public bool IsSeen {
            get { return _envelope.Flags.Any(x => x.ContainsIgnoreCase("\\Seen")); }
        }

        public async Task<string> FetchMessageBodyAsync() {
            using (var connection = new ImapConnection {Security = SecurityPolicies.Implicit}) {
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
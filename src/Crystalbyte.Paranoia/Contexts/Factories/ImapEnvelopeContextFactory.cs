#region Using directives

using System.Composition;
using Crystalbyte.Paranoia.Messaging;
using Crystalbyte.Paranoia.Models;

#endregion

namespace Crystalbyte.Paranoia.Contexts.Factories {
    [Export, Shared]
    public sealed class ImapEnvelopeContextFactory {
        public ImapMessageContext Create(ImapAccountContext account, ImapEnvelope envelope, string mailbox) {
            return new ImapMessageContext(account, envelope, mailbox);
        }
    }
}
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using System;
using System.Linq;

namespace Crystalbyte.Paranoia {
    public static class ImapEnvelopeExtensions {
        public static MailMessageModel ToMailMessage(this ImapEnvelope envelope) {
            return new MailMessageModel {
                EntryDate = envelope.InternalDate.HasValue
                    ? envelope.InternalDate.Value
                    : DateTime.Now,
                Subject = envelope.Subject,
                Flags = string.Join(";", envelope.Flags),
                Size = envelope.Size,
                Uid = envelope.Uid,
                MessageId = envelope.MessageId,
                FromAddress = envelope.From.Any()
                    ? envelope.From.First().Address
                    : string.Empty,
                FromName = envelope.From.Any()
                    ? envelope.From.First().DisplayName
                    : string.Empty
            };
        }
    }
}

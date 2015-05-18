using System;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;

namespace Crystalbyte.Paranoia {
    internal static class MailMessageReaderExtension {
        public static MailMessage ToMailMessage(this MailMessageReader reader) {
            var message = new MailMessage {
                EntryDate = DateTime.Parse(reader.Headers.Date),
                Subject = reader.Headers.Subject,
                Size = reader.RawMessage.Length,
                HasAttachments = reader.FindAllAttachments().Count > 0,
                FromName = reader.Headers.From.DisplayName,
                FromAddress = reader.Headers.From.Address,
                MessageId = reader.Headers.MessageId
            };

            return message;
        }
    }
}

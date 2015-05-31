using System;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;

namespace Crystalbyte.Paranoia {
    internal static class MailMessageReaderExtension {
        public static MailMessage ToMailMessage(this MailMessageReader reader) {
            var message = new MailMessage {
                Date = DateTime.Parse(reader.Headers.Date),
                Subject = reader.Headers.Subject,
                Size = reader.RawMessage.Length,
                MessageId = reader.Headers.MessageId
            };

            return message;
        }
    }
}

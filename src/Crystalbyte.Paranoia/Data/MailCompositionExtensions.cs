using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Data {
    internal static class MailCompositionExtensions {
        public static System.Net.Mail.MailMessage ToMessage(this MailComposition composition, string from) {
            var addresses = composition.Addresses
                    .Select(x => x.Address)
                    .ToArray();

            var message = new System.Net.Mail.MailMessage();
            message.To.AddRange(composition.Addresses.Where(x => x.Role == AddressRole.To).Select(x => new System.Net.Mail.MailAddress(x.Address)));
            message.CC.AddRange(composition.Addresses.Where(x => x.Role == AddressRole.Cc).Select(x => new System.Net.Mail.MailAddress(x.Address)));
            message.Bcc.AddRange(composition.Addresses.Where(x => x.Role == AddressRole.Bcc).Select(x => new System.Net.Mail.MailAddress(x.Address)));
            message.Subject = composition.Subject;

            message.From = new System.Net.Mail.MailAddress(from);
            message.Body = composition.Content;
            message.IsBodyHtml = true;

            return message;
        }
    }
}

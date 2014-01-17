using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Messaging {

    /// <summary>
    ///  Exposes the internal save method for the MailMessage.
    /// http://metdepuntnaarvoren.nl/create-eml-file-from-system.netmail.mailmessage
    /// </summary>
    public static class MailMessageExtensions {

        public static string ToMime(this MailMessage message) {
            var stream = new MemoryStream();
            var mailWriterType = message.GetType().Assembly.GetType("System.Net.Mail.MailWriter");
            var mailWriter = Activator.CreateInstance(mailWriterType, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { stream }, null, null);
            message.GetType().InvokeMember("Send", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, message, new[] { mailWriter, true, true });
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}

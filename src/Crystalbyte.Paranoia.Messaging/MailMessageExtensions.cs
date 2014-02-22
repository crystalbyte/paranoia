using Crystalbyte.Paranoia.Messaging.Mime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Messaging {

    /// <summary>
    ///  Exposes the internal save method for the MailMessage.
    /// http://metdepuntnaarvoren.nl/create-eml-file-from-system.netmail.mailmessage
    /// </summary>
    public static class MailMessageExtensions {

        public static Task<string> ToMimeAsync(this System.Net.Mail.MailMessage message) {
            return Task.Factory.StartNew(() => {
                var stream = new MemoryStream();
                var mailWriterType = message.GetType().Assembly.GetType("System.Net.Mail.MailWriter");
                var mailWriter = Activator.CreateInstance(mailWriterType, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { stream }, null, null);
                message.GetType().InvokeMember("Send", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, message, new[] { mailWriter, true, true });
                return Encoding.UTF8.GetString(stream.ToArray());
            });
        }
    }
}

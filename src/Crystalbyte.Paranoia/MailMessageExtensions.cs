using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.Mail;
using MailMessage = System.Net.Mail.MailMessage;
using System.Net.Mime;
using Crystalbyte.Paranoia.Data;

namespace Crystalbyte.Paranoia {
    internal static class MailMessageExtensions {

        private static readonly Random Random = new Random();

        public static void SetPublicKeys(this MailMessage message, MailContact contact) {
            foreach (var key in contact.Keys) {
                var k = Convert.ToBase64String(key.Bytes);
                message.Headers.Add(MessageHeaders.PublicKey, string.Format("v = 1; k = {0}; d = {1}", k, key.Device));
            }
        }

        public static async Task<MailMessage> WrapEncryptedMessageAsync(this MailMessage message, MimeEncryptionResult result) {
            var wrapper = new MailMessage {
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                BodyTransferEncoding = TransferEncoding.Base64,
            };

            wrapper.Headers.Add(MessageHeaders.Nonce, result.ToHeader());
            foreach (var entry in result.Entries) {
                wrapper.Headers.Add(MessageHeaders.AemKey, entry.ToString());
            }

            var resource = new Uri("/Resources/encryption-wrapper.html");
            // BUG: Occasionally throws ExecutionEngineException if not locked, so sad ... :(
            var info = Application.GetResourceStream(resource);
            if (info == null) {
                throw new ResourceNotFoundException(resource.AbsoluteUri);
            }

            var first = result.Entries.First();
            wrapper.To.Add(first.Contact.Address);
            wrapper.Subject = string.Format(Resources.SubjectTemplate, first.Contact.Name);
            wrapper.From = message.From;

            using (var reader = new StreamReader(info.Stream)) {
                wrapper.Body = await reader.ReadToEndAsync();
            }

            var mime = await message.ToMimeAsync();
            wrapper.AlternateViews.Add(new AlternateView(new MemoryStream(mime), new ContentType(MediaTypes.EncryptedMime)));
            return wrapper;
        }

        public static void PackageEmbeddedContent(this MailMessage message) {
            Application.Current.AssertBackgroundThread();

            var regex = new Regex("<img.+?src=\"(?<PATH>file:///.+?)\".*?>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            message.Body = regex.Replace(message.Body, m => {
                var time = Stopwatch.GetTimestamp();
                var random = Random.Next(0, 1000000);
                var path = m.Groups["PATH"].Value;
                var uri = new Uri(path, UriKind.Absolute);
                var info = new FileInfo(uri.LocalPath.Trim('/'));

                var name = info.Name;
                var bytes = File.ReadAllBytes(info.FullName);
                var domain = message.From.Address.Split('@').Last();
                var cid = string.Format("{0}.{1}@{2}", time, random, domain);

                var attachment = new Attachment(new MemoryStream(bytes), name,
                    name.GetMimeType()) {
                    ContentId = cid,
                    NameEncoding = Encoding.UTF8
                };

                message.Attachments.Add(attachment);

                var r = string.Format("cid:{0}", cid);
                return m.Value.Replace(path, r);
            });
        }

    }
}

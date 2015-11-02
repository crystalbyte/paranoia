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
using System.Collections.Generic;

namespace Crystalbyte.Paranoia {
    internal static class MailMessageExtensions {

        private static readonly Random Random = new Random();

        public static void AttachPublicKeys(this MailMessage message, IEnumerable<KeyPair> keys) {
            foreach (var key in keys) {
                var k = Convert.ToBase64String(key.PublicKey);
                message.Headers.Add(MessageHeaders.PublicKey, string.Format("k={0}; d={1}", k, key.Device));
            }
        }

        public static async Task<SodiumMimeDecryptionMetadata> UnwrapEncryptedMessageAsync(this MailMessage message) {
            throw new NotImplementedException();
            //var reader = new MailMessageReader();
            //var part = reader.FindFirstMessagePartWithMediaType(MessageHeaders.CypherVersion);

            //for (var i = 0; i < xHeaders.Count; i++) {
            //    var key = xHeaders.GetKey(i);
            //    if (!key.EqualsIgnoreCase(MessageHeaders.Signet))
            //        continue;

            //    var values = xHeaders.GetValues(i);
            //    if (values == null) {
            //        throw new SignetMissingOrCorruptException(address);
            //    }

            //    var signet = values.First();
            //    var split = signet.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            //    var p = split[0].Substring(split[0].IndexOf('=') + 1).Trim(';');
            //    pKey = Convert.FromBase64String(p);
            //}

            //var part = parts.First();
            //using (var context = new DatabaseContext()) {
            //    var current = context.KeyPairs.OrderByDescending(x => x.Date).First();
            //    var contact = context.MailContacts
            //        .Include(x => x.Keys)
            //        .FirstOrDefault(x => x.Address == address);

            //    if (contact == null) {
            //        throw new MissingContactException(address);
            //    }

            //    if (contact.Keys.Count == 0) {
            //        throw new MissingKeyException(address);
            //    }

            //    var crypto = new PublicKeyCrypto(current.PublicKey, current.PrivateKey);
            //    return crypto.DecryptWithPrivateKey(part.Body, pKey, nonce);
            //}

            //return null;
        }

        public static async Task<MailMessage> WrapSodiumEncryptedMessageAsync(this MailMessage message, SodiumMimeEncryptionMetadata data) {
            var wrapper = new MailMessage {
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                BodyTransferEncoding = TransferEncoding.Base64,
            };

            wrapper.Headers.Add(MessageHeaders.EncryptedMessage, data.ToMimeHeader());
            foreach (var entry in data.Entries) {
                wrapper.Headers.Add(MessageHeaders.Secret, entry.ToMimeHeader());
            }

            var resource = new Uri("/Resources/encryption-wrapper.html");
            // BUG: Occasionally throws ExecutionEngineException if not locked, so sad ... :(
            var info = Application.GetResourceStream(resource);
            if (info == null) {
                throw new ResourceNotFoundException(resource.AbsoluteUri);
            }

            var first = data.Entries.First();
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Crystalbyte.Paranoia.Data;
using MailMessage = Crystalbyte.Paranoia.Data.MailMessage;

namespace Crystalbyte.Paranoia {
    internal static class MailMessageExtensions {

        private static readonly Random Random = new Random();

        public static void PackageEmbeddedContent(this System.Net.Mail.MailMessage message) {
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

#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia
// 
// Crystalbyte.Paranoia is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License.
// 
// Foobar is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Using Directives

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

#endregion

namespace Crystalbyte.Paranoia {
    internal static class MailMessageExtension {
        private static readonly Random Random = new Random();

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
                                                               name.GetMimeType())
                                                           {
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
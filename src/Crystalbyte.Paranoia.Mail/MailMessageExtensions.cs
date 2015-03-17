#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia.Mail
// 
// Crystalbyte.Paranoia.Mail is free software: you can redistribute it and/or modify
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
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

#endregion

namespace Crystalbyte.Paranoia.Mail {
    /// <summary>
    ///     Exposes the internal save method for the MailMessage.
    ///     http://metdepuntnaarvoren.nl/create-eml-file-from-system.netmail.mailmessage
    /// </summary>
    public static class MailMessageExtensions {
        public static Task<string> ToMimeAsync(this MailMessage message) {
            return Task.Factory.StartNew(() => {
                                             var stream = new MemoryStream();
                                             var mailWriterType =
                                                 message.GetType().Assembly.GetType("System.Net.Mail.MailWriter");
                                             var mailWriter = Activator.CreateInstance(mailWriterType,
                                                 BindingFlags.Instance | BindingFlags.NonPublic, null,
                                                 new object[] {stream}, null, null);
                                             message.GetType()
                                                 .InvokeMember("Send",
                                                     BindingFlags.Instance | BindingFlags.NonPublic |
                                                     BindingFlags.InvokeMethod, null, message,
                                                     new[] {mailWriter, true, true});

                                             return Encoding.UTF8.GetString(stream.ToArray());
                                         });
        }
    }
}
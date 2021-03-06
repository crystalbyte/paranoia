﻿#region Copyright Notice & Copying Permission

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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Mail.Mime.Header;

#endregion

namespace Crystalbyte.Paranoia.Mail {
    public sealed class SmtpSession : IDisposable {
        private readonly SmtpAuthenticator _authenticator;
        private readonly SmtpConnection _connection;

        internal SmtpSession(SmtpAuthenticator authenticator) {
            _authenticator = authenticator;
            _connection = authenticator.Connection;
        }

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        public void OnProgressChanged(ProgressChangedEventArgs e) {
            var handler = ProgressChanged;
            if (handler != null)
                handler(this, e);
        }

        #region Implementation of IDisposable

        public void Dispose() {
            _authenticator.Dispose();
        }

        #endregion

        public Task SendAsync(MailMessage message) {
            return SendAsync(new[] {message});
        }

        private async Task SendAsync(byte[] mime) {
            var message = new MailMessageReader(mime);

            var from = message.Headers.From;
            var to = message.Headers.To;
            var cc = message.Headers.Cc;
            var bcc = message.Headers.Bcc;

            await _connection.WriteAsync(string.Format("{0} FROM:<{1}>", SmtpCommands.Mail, from.Address));
            var response = await _connection.ReadAsync();
            if (response.IsError) {
                throw new SmtpException(response.Content);
            }

            var target = to.Concat(cc).Concat(bcc).Distinct(new RfcMailAddressComparer());
            foreach (var contact in target) {
                await _connection.WriteAsync(string.Format("{0} TO:<{1}>", SmtpCommands.Rcpt, contact.Address));
                response = await _connection.ReadAsync();
                if (response.IsError) {
                    throw new SmtpException(response.Content);
                }
            }

            await SendDataAsync(mime);

            response = await _connection.ReadAsync();
            if (!response.IsOk) {
                throw new SmtpException(response.Content);
            }
        }

        public async Task SendAsync(IEnumerable<MailMessage> messages) {
            foreach (var message in messages) {
                await _connection.WriteAsync(string.Format("{0} FROM:<{1}>", SmtpCommands.Mail, message.From.Address));
                var response = await _connection.ReadAsync();
                if (response.IsError) {
                    throw new SmtpException(response.Content);
                }

                var target = message.To.Concat(message.CC).Concat(message.Bcc).Distinct(new MailAddressComparer());
                foreach (var contact in target) {
                    await _connection.WriteAsync(string.Format("{0} TO:<{1}>", SmtpCommands.Rcpt, contact.Address));
                    response = await _connection.ReadAsync();
                    if (response.IsError) {
                        throw new SmtpException(response.Content);
                    }
                }

                await SendDataAsync(message);
                response = await _connection.ReadAsync();
                if (!response.IsOk) {
                    throw new SmtpException(response.Content);
                }
            }

            await _connection.WriteAsync(SmtpCommands.Quit);
            var confirmation = await _connection.ReadAsync();
            if (confirmation.IsError) {
                throw new SmtpException(confirmation.Content);
            }
        }

        private async Task SendDataAsync(MailMessage message) {
            var mime = await message.ToMimeAsync();
            await SendDataAsync(mime);
        }

        private async Task SendDataAsync(byte[] mime) {
            await _connection.WriteAsync(SmtpCommands.Data);
            var response = await _connection.ReadAsync();
            if (!response.IsContinuationRequest) {
                throw new SmtpException(response.Content);
            }

            var total = mime.Length;
            long bytes = 0;

            var mimeString = Encoding.UTF8.GetString(mime);

            using (var reader = new StringReader(mimeString)) {
                while (true) {
                    var line = StuffPeriodIfNecessary(await reader.ReadLineAsync());

                    if (string.IsNullOrEmpty(line)) {
                        bytes += 2;
                        await _connection.WriteAsync(string.Empty);
                        continue;
                    }

                    await _connection.WriteAsync(line);

                    // Add two for line termination symbols \r\n.
                    bytes += Encoding.UTF8.GetByteCount(line) + 2;
                    OnProgressChanged(new ProgressChangedEventArgs(bytes));

                    if (total <= bytes) {
                        break;
                    }
                }
            }

            await _connection.WriteAsync(".");
        }

        private static string StuffPeriodIfNecessary(string line) {
            if (string.IsNullOrEmpty(line)) {
                return null;
            }

            if (line.StartsWith(".")) {
                line = line.Insert(0, ".");
            }
            return line;
        }


        private class MailAddressComparer : IEqualityComparer<MailAddress> {
            #region Implementation of IEqualityComparer<in MailAddress>

            public bool Equals(MailAddress x, MailAddress y) {
                return x.Address == y.Address;
            }

            public int GetHashCode(MailAddress obj) {
                return obj.Address.GetHashCode() ^ 13;
            }

            #endregion
        }

        private class RfcMailAddressComparer : IEqualityComparer<RfcMailAddress> {
            #region Implementation of IEqualityComparer<in RfcMailAddress>

            public bool Equals(RfcMailAddress x, RfcMailAddress y) {
                return x.Address == y.Address;
            }

            public int GetHashCode(RfcMailAddress obj) {
                return obj.Address.GetHashCode() ^ 13;
            }

            #endregion
        }
    }
}
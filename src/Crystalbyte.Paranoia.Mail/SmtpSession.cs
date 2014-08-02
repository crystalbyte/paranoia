#region Using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

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

        public Task SendAsync(System.Net.Mail.MailMessage message) {
            return SendAsync(new[] { message });
        }

        public async Task SendAsync(IEnumerable<System.Net.Mail.MailMessage> messages) {
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

        private async Task SendDataAsync(System.Net.Mail.MailMessage message) {
            await _connection.WriteAsync(SmtpCommands.Data);
            var response = await _connection.ReadAsync();
            if (!response.IsContinuationRequest) {
                throw new SmtpException(response.Content);
            }

            var mime = await message.ToMimeAsync();
            var total = Encoding.UTF8.GetByteCount(mime);
            long bytes = 0;

            using (var reader = new StringReader(mime)) {
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

                    if (total == bytes) {
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
    }
}
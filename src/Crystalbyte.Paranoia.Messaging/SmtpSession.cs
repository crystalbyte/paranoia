using System.Net;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Messaging {
    public sealed class SmtpSession : IDisposable {
        private readonly string _host;
        private readonly int _port;
        private readonly SmtpCredentials _credentials;
        private readonly SmtpClient _client;

        public SmtpSession(string host, int port, SmtpCredentials credentials) {
            _host = host;
            _port = port;
            _credentials = credentials;
            _client = new SmtpClient();
        }

        public bool IsSslEnabled { get; set; }

        public async Task SendAsync(MailMessage message) {
            _client.UseDefaultCredentials = false;
            _client.Credentials= new NetworkCredential(_credentials.Username, _credentials.Password);
            _client.EnableSsl = IsSslEnabled;

            _client.Host = _host;
            _client.Port = _port;

            await _client.SendMailAsync(message);
        }

        #region Implementation of IDisposable

        public void Dispose() {
            if (_client != null) {
                _client.Dispose();
            }
        }

        #endregion
    }
}

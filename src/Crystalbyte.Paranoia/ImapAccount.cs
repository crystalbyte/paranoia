using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using ActiveUp.Net.Mail;
using ActiveUp.Net.Security;

namespace Crystalbyte.Paranoia {
    public sealed class ImapAccount {
        private Imap4Client _client;

        public Task ConnectAsync() {
            _client = new Imap4Client();

            return Task.Factory.StartNew(() => {
                if (SecurityProtocol == SecurityProtocol.Ssl3) {
                    _client.ConnectSsl(Host, Port);
                } else {
                    _client.Connect(Host, Port);

                    var capabilities = _client.ServerCapabilities.Contains("STARTTLS");
                }
            });
        }



        public void Disconnect() {
            _client.Disconnect();
        }

        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public short Port { get; set; }
        public SecurityProtocol SecurityProtocol { get; set; }
    }
}

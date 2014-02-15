﻿using Crystalbyte.Paranoia.Messaging.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Messaging {
    public sealed class SmtpAuthenticator : IDisposable {
        private readonly SmtpConnection _connection;

        internal SmtpAuthenticator(SmtpConnection connection) {
            _connection = connection;
        }

        internal SmtpConnection Connection {
            get { return _connection; }
        }

        public async Task<SmtpSession> LoginAsync(string username, string password) {
            var auth = _connection.Capabilities.FirstOrDefault(x => x.ToUpper().StartsWith("AUTH"));

            if (string.IsNullOrWhiteSpace(auth)) {
                // No auth is advertised
                return new SmtpSession(this);
            }

            var mechanics = auth.Split(' ')
                .Where((x, i) => i != 0)
                .Select(x => x.ToUpper())
                .ToArray();

            if (mechanics.Contains("PLAIN")) {
                await AuthPlainAsync(username, password);
                return new SmtpSession(this);
            }

            if (mechanics.Contains("LOGIN")) {
                await AuthLoginAsync(username, password);
                return new SmtpSession(this);
            }

            throw new NotSupportedException("Other mechanics than PLAIN and LOGIN are currently not supported.");
        }

        private async Task AuthLoginAsync(string username, string password) {
            var b1 = Encoding.UTF8.GetBytes(username);
            var l1 = Convert.ToBase64String(b1);

            await _connection.WriteAsync(string.Format("AUTH LOGIN {0}", l1));
            while (true) {
                var line = await _connection.ReadAsync();
                if (!line.IsPasswordRequest) {
                    throw new SmtpException(SmtpStatusCode.BadCommandSequence);
                }

                var b2 = Encoding.UTF8.GetBytes(password);
                var l2 = Convert.ToBase64String(b2);
                await _connection.WriteAsync(l2);

                line = await _connection.ReadAsync();
                if (!line.IsTerminated) {
                    throw new SmtpException(SmtpStatusCode.BadCommandSequence);
                }

                return;
            }
        }


        /// <summary>
        ///   Plain authorization.
        ///   http://tools.ietf.org/html/rfc4616
        /// </summary>
        private async Task AuthPlainAsync(string username, string password) {
            var seed = username + "\0" + username + "\0" + password;

            var bytes = Encoding.UTF8.GetBytes(seed);
            var challenge = Convert.ToBase64String(bytes);

            await _connection.WriteAsync(string.Format("AUTH PLAIN {0}", challenge));
            while (true) {
                var line = await _connection.ReadAsync();
                if (!line.IsAuthenticated) {
                    throw new AuthenticationException(Resources.AuthenticationFailedMessage);
                }

                if (line.IsTerminated) {
                    break;
                }
            }
        }

        public void Dispose() {
            _connection.Dispose();
        }
    }
}

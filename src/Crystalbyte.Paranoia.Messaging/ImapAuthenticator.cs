using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Messaging {
    public sealed class ImapAuthenticator : IDisposable {
        private readonly ImapConnection _connection;

        internal ImapAuthenticator(ImapConnection connection) {
            _connection = connection;
        }

        internal ImapConnection Connection {
            get { return _connection; }
        }

        public async Task<ImapSession> LoginAsync(string username, string password) {
            if (!_connection.Capabilities.Contains("AUTH=PLAIN")) {
                throw new NotSupportedException("Other mechanics than PLAIN are currently not supported.");
            }
            
            await AuthPlainAsync(username, password);
            return new ImapSession(this);
        }

        private async Task AuthPlainAsync(string username, string password) {
            var seed = username + "\0" + username + "\0" + password;

            var bytes = Encoding.UTF8.GetBytes(seed.ToCharArray());
            var hash = Convert.ToBase64String(bytes);

            string commandId;
            ResponseLine line;

            // Speed up authentication by using the initial client response extension.
            // http://tools.ietf.org/html/rfc4959
            if (_connection.Capabilities.Contains("SASL-IR")) {
                var command = string.Format("AUTHENTICATE PLAIN {0}", hash);
                commandId = await _connection.WriteCommandAsync(command);
                line = await _connection.ReadAsync();
            } else {
                var command = string.Format("AUTHENTICATE PLAIN");
                commandId = await _connection.WriteCommandAsync(command);
                var response = await _connection.ReadAsync();

                if (!response.IsContinuationRequest) {
                    throw new ImapException("Unexpected server response.");
                }

                await _connection.WriteAsync(hash);
                line = await _connection.ReadAsync();
            }

            if (line.Text.ContainsIgnoreCase("CAPABILITY")) {
                _connection.Capabilities = await _connection.ReadCapabilitiesAsync(commandId);
            }
        }

        public async void LogoutAsync() {
            var id = await _connection.WriteCommandAsync("LOGOUT");
            await _connection.TerminateCommandAsync(id);
        }

        #region Implementation of IDisposable

        public void Dispose() {
            if (_connection != null) {
                _connection.Dispose();
            }
        }

        #endregion
    }
}

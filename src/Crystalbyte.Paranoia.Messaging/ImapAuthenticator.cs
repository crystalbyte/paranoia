using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Messaging {
    public sealed class ImapAuthenticator : IDisposable {
        private readonly HashSet<string> _capabilities;
        private readonly ImapConnection _connection;

        internal ImapAuthenticator(HashSet<string> capabilities, ImapConnection connection) {
            _capabilities = capabilities;
            _connection = connection;
        }

        internal ImapConnection Connection {
            get { return _connection; }
        }

        public HashSet<string> Capabilities {
            get { return _capabilities; }
        }

        public async Task<bool> LoginAsync(string username, string password) {
            if (!_capabilities.Contains("AUTH=PLAIN")) {
                throw new NotSupportedException("Other mechanics than PLAIN are currently not supported.");
            }

            var response = await AuthPlainAsync(username, password);
            return response.IsOk;
        }

        private async Task<ResponseLine> AuthPlainAsync(string username, string password) {
            var seed = username + "\0" + username + "\0" + password;

            var bytes = Encoding.UTF8.GetBytes(seed.ToCharArray());
            var hash = Convert.ToBase64String(bytes);

            string command;
            if (_capabilities.Contains("SASL-IR")) {
                command = string.Format("AUTHENTICATE PLAIN {0}", hash);
                await _connection.WriteCommandAsync(command);
                return await _connection.ReadAsync();
            }

            command = string.Format("AUTHENTICATE PLAIN");
            await _connection.WriteCommandAsync(command);
            var response = await _connection.ReadAsync();
            if (response.IsContinuation) {
                await _connection.WriteAsync(hash);
                return await _connection.ReadAsync();
            }

            throw new ImapException("Unexpected server response.");
        }

        public void Logout() {
            
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

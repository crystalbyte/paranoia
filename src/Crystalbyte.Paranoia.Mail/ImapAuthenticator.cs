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
using System.Text;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Mail.Properties;

#endregion

namespace Crystalbyte.Paranoia.Mail {
    public sealed class ImapAuthenticator : IDisposable {
        private readonly ImapConnection _connection;

        internal ImapAuthenticator(ImapConnection connection) {
            _connection = connection;
        }

        internal ImapConnection Connection {
            get { return _connection; }
        }

        public async Task<ImapSession> LoginAsync(string username, string password) {
            if (_connection.Capabilities.Contains("AUTH=PLAIN")) {
                await AuthPlainAsync(username, password);
                return new ImapSession(this);
            }

            await AuthLoginAsync(username, password);
            return new ImapSession(this);
        }

        private async Task AuthLoginAsync(string username, string password) {
            var text = string.Format("{0} {1}", username, password);

            // Speed up authentication by using the initial client response extension.
            // http://tools.ietf.org/html/rfc4959
            var command = string.Format("LOGIN {0}", text);
            await _connection.WriteCommandAsync(command);
            var line = await _connection.ReadAsync();

            if (line.IsNo) {
                throw new AuthenticationException(Resources.AuthenticationFailedMessage);
            }
        }

        private async Task AuthPlainAsync(string username, string password) {
            var seed = username + "\0" + username + "\0" + password;

            var bytes = Encoding.UTF8.GetBytes(seed.ToCharArray());
            var hash = Convert.ToBase64String(bytes);

            ImapResponseLine line;

            // Speed up authentication by using the initial client response extension.
            // http://tools.ietf.org/html/rfc4959
            if (_connection.Capabilities.Contains("SASL-IR")) {
                var command = string.Format("AUTHENTICATE PLAIN {0}", hash);
                await _connection.WriteCommandAsync(command);
                line = await _connection.ReadAsync();
            }
            else {
                var command = string.Format("AUTHENTICATE PLAIN");
                await _connection.WriteCommandAsync(command);
                var response = await _connection.ReadAsync();

                if (!response.IsContinuationRequest) {
                    throw new ImapException("Unexpected server response.");
                }

                await _connection.WriteAsync(hash);
                line = await _connection.ReadAsync();
            }

            if (line.IsNo) {
                throw new AuthenticationException(Resources.AuthenticationFailedMessage);
            }
        }

        public async Task LogoutAsync() {
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
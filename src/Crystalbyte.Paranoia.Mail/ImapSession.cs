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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#endregion

namespace Crystalbyte.Paranoia.Mail {
    public sealed class ImapSession : IDisposable {
        private readonly ImapAuthenticator _authenticator;
        private readonly ImapConnection _connection;

        internal ImapSession(ImapAuthenticator authenticator) {
            _authenticator = authenticator;
            _connection = authenticator.Connection;
        }

        internal ImapAuthenticator Authenticator {
            get { return _authenticator; }
        }

        /// <summary>
        ///     The LIST command returns a subset of names from the complete set of all names available to the client.
        ///     http://tools.ietf.org/html/rfc3501#section-6.3.8
        /// </summary>
        /// <param name="referenceName">The reference name.</param>
        /// <param name="wildcardedMailboxName">The mailbox name with possible wildcards.</param>
        public async Task<List<ImapMailboxInfo>> ListAsync(string referenceName, string wildcardedMailboxName) {
            var command = string.Format("{0} \"{1}\" \"{2}\"", ImapCommands.List, referenceName, wildcardedMailboxName);
            var id = await _connection.WriteCommandAsync(command);
            return await ReadListResponseAsync(id);
        }

        /// <summary>
        ///     The LIST command returns a subset of names from the complete set of all names available to the client.
        ///     http://tools.ietf.org/html/rfc3501#section-6.3.9
        /// </summary>
        /// <param name="referenceName">The reference name.</param>
        /// <param name="wildcardedMailboxName">The mailbox name with possible wildcards.</param>
        public async Task<List<ImapMailboxInfo>> LSubAsync(string referenceName, string wildcardedMailboxName) {
            var command = string.Format("{0} \"{1}\" \"{2}\"", ImapCommands.LSub, referenceName, wildcardedMailboxName);
            var id = await _connection.WriteCommandAsync(command);
            return await ReadListResponseAsync(id);
        }

        /// <summary>
        ///     The SUBSCRIBE command adds the specified mailbox name to the
        ///     server's set of "active" or "subscribed" mailboxes as returned by
        ///     the LSUB command.  This command returns a tagged OK response only
        ///     if the subscription is successful.
        ///     http://tools.ietf.org/html/rfc3501#section-6.3.6
        /// </summary>
        /// <param name="name">mailbox</param>
        /// <returns>no specific responses for this command</returns>
        public async Task SubscribeAsync(string name) {
            // we need to convert non ASCII names according to IMAP specs.
            // http://tools.ietf.org/html/rfc2060#section-5.1.3
            var encodedName = ImapMailbox.EncodeName(name);

            var command = string.Format("{0} \"{1}\"", ImapCommands.Subscribe, encodedName);
            var id = await _connection.WriteCommandAsync(command);
            await ReadNamespaceResponseAsync(id);
        }

        private async Task ReadNamespaceResponseAsync(string id) {
            while (true) {
                var line = await _connection.ReadAsync();

                if (line.IsUntagged) {
                    Debug.WriteLine(line.Text);
                }

                if (!line.TerminatesCommand(id))
                    continue;

                if (!line.IsOk) {
                    throw new ImapException(line.Text);
                }

                break;
            }
        }

        /// <summary>
        ///     The UNSUBSCRIBE command removes the specified mailbox name from
        ///     the server's set of "active" or "subscribed" mailboxes as returned
        ///     by the LSUB command.  This command returns a tagged OK response
        ///     only if the unsubscription is successful.
        ///     http://tools.ietf.org/html/rfc3501#section-6.3.7
        /// </summary>
        /// <param name="name">mailbox name</param>
        /// <returns>no specific responses for this command</returns>
        public async Task UnsubscribeAsync(string name) {
            // we need to convert non ASCII names according to IMAP specs.
            // http://tools.ietf.org/html/rfc2060#section-5.1.3
            var encodedName = ImapMailbox.EncodeName(name);

            var command = string.Format("{0} \"{1}\"", ImapCommands.Unsubscribe, encodedName);
            var id = await _connection.WriteCommandAsync(command);
            await ReadBasicResponseAsync(id);
        }

        public async Task GetNamespacesAsync() {
            var command = ImapCommands.Namespace;
            var id = await _connection.WriteCommandAsync(command);
            await ReadBasicResponseAsync(id);
        }

        public async Task DeleteMailboxAsync(string mailbox) {
            var encodedName = ImapMailbox.EncodeName(mailbox);
            var command = string.Format("DELETE \"{0}\"", encodedName);
            var id = await _connection.WriteCommandAsync(command);
            await ReadBasicResponseAsync(id);
        }

        public async Task CreateMailboxAsync(string fullname) {
            var encodedName = ImapMailbox.EncodeName(fullname);
            var command = string.Format("CREATE \"{0}\"", encodedName);
            var id = await _connection.WriteCommandAsync(command);
            await ReadBasicResponseAsync(id);
        }

        private async Task ReadBasicResponseAsync(string id) {
            while (true) {
                var line = await _connection.ReadAsync();
                if (!line.TerminatesCommand(id))
                    continue;

                if (!line.IsOk) {
                    throw new ImapException(line.Text);
                }

                break;
            }
        }

        private async Task<List<ImapMailboxInfo>> ReadListResponseAsync(string id) {
            var mailboxes = new List<ImapMailboxInfo>();
            while (true) {
                var line = await _connection.ReadAsync();
                if (line.IsUntagged) {
                    mailboxes.Add(ImapMailboxInfo.Parse(line));
                }

                if (line.TerminatesCommand(id)) {
                    break;
                }
            }
            return mailboxes;
        }

        public async Task<ImapMailbox> SelectAsync(string name) {
            // we need to convert non ASCII names according to IMAP specs.
            // http://tools.ietf.org/html/rfc2060#section-5.1.3
            var encodedName = ImapMailbox.EncodeName(name);

            var command = string.Format("{0} \"{1}\"", ImapCommands.Select, encodedName);
            var id = await _connection.WriteCommandAsync(command);
            return await ReadSelectResponseAsync(name, id);
        }

        public async Task<ImapMailbox> ExamineAsync(string name) {
            // we need to convert non ASCII names according to IMAP specs.
            // http://tools.ietf.org/html/rfc2060#section-5.1.3
            var encodedName = ImapMailbox.EncodeName(name);


            var command = string.Format("{0} \"{1}\"", ImapCommands.Examine, encodedName);
            var id = await _connection.WriteCommandAsync(command);
            return await ReadSelectResponseAsync(name, id);
        }

        private async Task<ImapMailbox> ReadSelectResponseAsync(string name, string commandId) {
            var mailbox = new ImapMailbox(this, name);
            while (true) {
                var line = await _connection.ReadAsync();
                if (line.TerminatesCommand(commandId)) {
                    if (line.Text.ContainsIgnoreCase("[READ-WRITE]")) {
                        mailbox.Permissions = MailboxPermissions.Read | MailboxPermissions.Write;
                    }
                    break;
                }

                if (line.IsUntagged && !line.IsUntaggedOk && line.Text.ContainsIgnoreCase("FLAGS")) {
                    const string pattern = @"\\[A-za-z0-9\*]+";
                    mailbox.AddFlags(Regex.Matches(line.Text, pattern, RegexOptions.CultureInvariant)
                        .Cast<Match>()
                        .Select(x => x.Value));
                }

                if (line.IsUntaggedOk && line.Text.ContainsIgnoreCase("PERMANENTFLAGS")) {
                    const string pattern = @"\\[A-za-z0-9\*]+";
                    mailbox.AddPermanentFlags(Regex.Matches(line.Text, pattern, RegexOptions.CultureInvariant)
                        .Cast<Match>()
                        .Select(x => x.Value));
                    continue;
                }

                if (line.IsUntaggedOk && line.Text.ContainsIgnoreCase("UIDVALIDITY")) {
                    const string pattern = @"[0-9]+";
                    var value = Regex.Match(line.Text, pattern, RegexOptions.CultureInvariant).Value;
                    mailbox.UidValidity = long.Parse(value);
                    continue;
                }

                if (line.IsUntagged && Regex.IsMatch(line.Text, @"\d+ EXISTS", RegexOptions.IgnoreCase)) {
                    const string pattern = @"[0-9]+";
                    var value = Regex.Match(line.Text, pattern, RegexOptions.CultureInvariant).Value;
                    mailbox.Exists = int.Parse(value);
                    continue;
                }

                if (line.IsUntagged && Regex.IsMatch(line.Text, @"\d+ RECENT", RegexOptions.IgnoreCase)) {
                    const string pattern = @"[0-9]+";
                    var value = Regex.Match(line.Text, pattern, RegexOptions.CultureInvariant).Value;
                    mailbox.Recent = int.Parse(value);
                    continue;
                }

                if (line.IsUntagged && line.Text.ContainsIgnoreCase("UIDNEXT")) {
                    const string pattern = @"[0-9]+";
                    var value = Regex.Match(line.Text, pattern, RegexOptions.CultureInvariant).Value;
                    mailbox.UidNext = int.Parse(value);
                }
            }

            return mailbox;
        }

        #region Implementation of IDisposable

        public void Dispose() {
            if (_authenticator != null) {
                _authenticator.Dispose();
            }
        }

        #endregion
    }
}
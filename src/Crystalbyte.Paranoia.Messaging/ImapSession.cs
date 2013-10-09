#region Using directives

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#endregion

namespace Crystalbyte.Paranoia.Messaging {
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

        public async Task<ImapMailbox> SelectAsync(string name) {
            // we need to convert non ASCII names according to IMAP specs.
            // http://tools.ietf.org/html/rfc2060#section-5.1.3
            var encodedName = ImapMailbox.EncodeName(name);

            var command = string.Format("SELECT \"{0}\"", encodedName);
            var id = await _connection.WriteCommandAsync(command);
            return await ReadSelectResponseAsync(name, id);
        }

        public async Task<ImapMailbox> ExamineAsync(string name) {
            // we need to convert non ASCII names according to IMAP specs.
            // http://tools.ietf.org/html/rfc2060#section-5.1.3
            var encodedName = ImapMailbox.EncodeName(name);

            var command = string.Format("EXAMINE \"{0}\"", encodedName);
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

                if (line.IsUntagged && line.Text.ContainsIgnoreCase("EXISTS")) {
                    const string pattern = @"[0-9]+";
                    var value = Regex.Match(line.Text, pattern, RegexOptions.CultureInvariant).Value;
                    mailbox.Exists = int.Parse(value);
                    continue;
                }

                if (line.IsUntagged && line.Text.ContainsIgnoreCase("RECENT")) {
                    const string pattern = @"[0-9]+";
                    var value = Regex.Match(line.Text, pattern, RegexOptions.CultureInvariant).Value;
                    mailbox.Recent = int.Parse(value);
                    continue;
                }

                if (line.IsUntagged && line.Text.ContainsIgnoreCase("UIDNEXT")) {
                    const string pattern = @"[0-9]+";
                    var value = Regex.Match(line.Text, pattern, RegexOptions.CultureInvariant).Value;
                    mailbox.UidNext = int.Parse(value);
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
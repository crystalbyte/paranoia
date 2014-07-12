#region Using directives

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Mail.Properties;

#endregion

namespace Crystalbyte.Paranoia.Mail {
    public sealed class ImapMailbox {
        private readonly string _name;
        private readonly ImapConnection _connection;
        private readonly List<string> _permanentFlags;
        private readonly List<string> _flags;

        public ImapMailbox(ImapSession session, string name) {
            _name = name;
            _connection = session.Authenticator.Connection;
            _permanentFlags = new List<string>();
            _flags = new List<string>();
        }

        public event EventHandler MessageReceived;

        public void OnMessageReceived(EventArgs e) {
            var handler = MessageReceived;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        private void OnProgressChanged(long byteCount) {
            var handler = ProgressChanged;
            if (handler != null) {
                handler(this, new ProgressChangedEventArgs(byteCount));
            }
        }

        /// <summary>
        ///     The SEARCH command searches the mailbox for messages that match
        ///     the given searching criteria.  Searching criteria consist of one
        ///     or more search keys.
        /// </summary>
        /// <param name="criteria"> The search criteria. </param>
        /// <returns> The uids of messages matching the given criteria. </returns>
        public async Task<IList<int>> SearchAsync(string criteria) {
            var command = String.Format("UID SEARCH {0}", criteria);
            var id = await _connection.WriteCommandAsync(command);
            return await ReadSearchResponseAsync(id);
        }

        public async Task DeleteMailsAsync(IEnumerable<long> uids, string trashMailbox = "") {
            var uidString = uids.ToCommaSeparatedValues();
            var command = string.Format(@"UID STORE {0} +FLAGS.SILENT (\Deleted)", uidString);
            var id = await _connection.WriteCommandAsync(command);
            await ReadStoreResponseAsync(id);

            if (string.IsNullOrWhiteSpace(trashMailbox)) {
                return;
            }

            var encodedName = EncodeName(trashMailbox);
            if (_connection.CanMove) {
                command = string.Format("UID MOVE {0} \"{1}\"", uidString, encodedName);
                id = await _connection.WriteCommandAsync(command);
                await ReadMoveResponseAsync(id);
            }
            else {
                command = string.Format("UID COPY {0} \"{1}\"", uidString, encodedName);
                id = await _connection.WriteCommandAsync(command);
                await ReadCopyResponseAsync(id);

                id = await _connection.WriteCommandAsync("EXPUNGE");
                await ReadExpungeResponseAsync(id);
            }
        }

        private async Task ReadMoveResponseAsync(string id) {
            while (true) {
                var line = await _connection.ReadAsync();
                if (line.IsBad || line.IsNo) {
                    throw new ImapException(line.Text);
                }

                if (line.TerminatesCommand(id)) {
                    break;
                }
            }
        }

        private async Task ReadExpungeResponseAsync(string id) {
            while (true) {
                var line = await _connection.ReadAsync();
                if (line.IsBad || line.IsNo) {
                    throw new ImapException(line.Text);
                }

                if (line.TerminatesCommand(id)) {
                    break;
                }
            }
        }

        private async Task ReadCopyResponseAsync(string id) {
            while (true) {
                var line = await _connection.ReadAsync();
                if (line.IsBad || line.IsNo) {
                    throw new ImapException(line.Text);
                }

                if (line.TerminatesCommand(id)) {
                    break;
                }
            }
        }

        public string Name {
            get { return _name; }
        }

        private async Task ReadStoreResponseAsync(string id) {
            while (true) {
                var line = await _connection.ReadAsync();
                if (line.TerminatesCommand(id)) {
                    break;
                }
            }
        }

        /// <summary>
        ///     This method is blocking.
        ///     The IDLE command may be used with any IMAP4 server implementation
        ///     that returns "IDLE" as one of the supported capabilities to the
        ///     CAPABILITY command.  If the server does not advertise the IDLE
        ///     capability, the client MUST NOT use the IDLE command and must poll
        ///     for mailbox updates.
        ///     http://tools.ietf.org/html/rfc2177
        /// </summary>
        public async void Idle() {
            if (!_connection.Capabilities.Contains(ImapCommands.Idle)) {
                throw new NotSupportedException(Resources.NotSupportedImapCommandMessage);
            }

            await _connection.WriteCommandAsync(ImapCommands.Idle);
            IsIdle = true;

            var response = await _connection.ReadAsync();
            if (!response.IsContinuationRequest) {
                throw new ImapException(response.Text);
            }

            while (true) {
                var line = await _connection.ReadAsync();
                if (line.IsUntagged) {
                    HandlePushNotification(line);
                }
            }
        }

        private void HandlePushNotification(ImapResponseLine line) {
            if (line.Text.Contains(ImapResponses.Exists)) {
                OnMessageReceived(EventArgs.Empty);
                Exists = int.Parse(Regex.Match(line.Text, "[0-9]+").Value);
            }
        }

        public async Task StopIdleAsync() {
            await _connection.WriteCommandAsync(ImapCommands.Done);
            IsIdle = false;
        }

        private async Task<IList<int>> ReadSearchResponseAsync(string commandId) {
            var list = new List<int>();
            while (true) {
                var line = await _connection.ReadAsync();
                if (line.TerminatesCommand(commandId)) {
                    break;
                }

                // Not sure if this can ever happen.
                if (!line.IsUntagged)
                    continue;

                const string pattern = @"[0-9]+";
                var matches = Regex.Matches(line.Text, pattern, RegexOptions.CultureInvariant);
                list.AddRange(from Match match in matches select Int32.Parse(match.Value));
            }

            return list;
        }

        public bool IsIdle { get; private set; }
        public int UidNext { get; internal set; }
        public int Recent { get; internal set; }
        public int Exists { get; internal set; }
        public long UidValidity { get; internal set; }
        public MailboxPermissions Permissions { get; internal set; }

        public IEnumerable<string> PermanentFlags {
            get { return _permanentFlags; }
        }

        public IEnumerable<string> Flags {
            get { return _flags; }
        }

        internal void AddPermanentFlags(IEnumerable<string> flags) {
            _permanentFlags.AddRange(flags);
        }

        internal void AddFlags(IEnumerable<string> flags) {
            _flags.AddRange(flags);
        }

        public static string EncodeName(string text) {
            return Regex.Replace(text, RegexPatterns.NonBase64CharactersPattern, OnEncodingMatchEvaluation);
        }

        private static string OnEncodingMatchEvaluation(Match match) {
            if (match.Value == "&") {
                return "&-";
            }

            var bytes = Encoding.UTF7.GetBytes(match.Value.ToCharArray());
            var encodedString = Encoding.UTF8.GetString(bytes);
            encodedString = "&" + encodedString.TrimAny(1) + "-";
            return encodedString.Replace('/', ',');
        }

        public static string DecodeName(string text) {
            return Regex.Replace(text, RegexPatterns.Rfc2060ModifiedBase64Pattern, OnDecodingMatchEvaluation);
        }

        private static string OnDecodingMatchEvaluation(Match match) {
            if (match.Value == "&-") {
                return "&";
            }

            var text = match.Value.TrimAny(1);
            text = "+" + text + "-";
            text = text.Replace(',', '/');
            var bytes = Encoding.UTF8.GetBytes(text);
            return Encoding.UTF7.GetString(bytes);
        }

        public async Task<IEnumerable<ImapEnvelope>> FetchEnvelopesAsync(IEnumerable<int> uids) {
            var command = string.Format("UID FETCH {0} ALL", uids
                .Select(x => x.ToString(CultureInfo.InvariantCulture))
                .Aggregate((c, n) => c + ',' + n));

            var id = await _connection.WriteCommandAsync(command);
            return await ReadFetchEnvelopesResponseAsync(id);
        }

        private async Task<IEnumerable<ImapEnvelope>> ReadFetchEnvelopesResponseAsync(string commandId) {
            var segments = new List<string>();
            var lines = new List<ImapResponseLine> { await _connection.ReadAsync() };

            while (true) {
                var line = await _connection.ReadAsync();
                if (line.IsUntagged || line.TerminatesCommand(commandId)) {
                    using (var writer = new StringWriter()) {
                        foreach (var l in lines) {
                            await writer.WriteAsync(l.Text);
                            await writer.WriteLineAsync();
                        }
                        segments.Add(writer.ToString());
                    }
                    lines.Clear();
                }
                if (line.TerminatesCommand(commandId)) {
                    break;
                }

                lines.Add(line);
            }

            return segments.Select(ImapEnvelope.Parse).ToList();
        }

        public async Task<string> FetchMessageBodyAsync(long uid) {
            var command = string.Format("UID FETCH {0} BODY[]", uid);
            var id = await _connection.WriteCommandAsync(command);
            return await ReadMessageBodyResponseAsync(id);
        }

        private async Task<string> ReadMessageBodyResponseAsync(string id) {
            OnProgressChanged(0);
            using (var writer = new StringWriter()) {
                while (true) {
                    var line = await _connection.ReadAsync();
                    OnProgressChanged(Encoding.UTF8.GetByteCount(line.Text));
                    if (line.IsUntagged) {
                        continue;
                    }
                    if (line.TerminatesCommand(id)) {
                        break;
                    }
                    writer.WriteLine(line.Text);
                }
                return writer.ToString();
            }
        }
    }
}
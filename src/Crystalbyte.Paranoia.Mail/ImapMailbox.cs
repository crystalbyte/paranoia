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

        private const string UidPattern = @"UID \d+";
        private static readonly Regex UidRegex = new Regex(UidPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public ImapMailbox(ImapSession session, string name) {
            _name = name;
            _connection = session.Authenticator.Connection;
            _permanentFlags = new List<string>();
            _flags = new List<string>();
        }

        public event EventHandler ChangeNotificationReceived;

        private void OnChangeNotificationReceived() {
            var handler = ChangeNotificationReceived;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public event EventHandler<ProgressChangedEventArgs> ByteCountChanged;

        private void OnByteCountChanged(long byteCount) {
            var handler = ByteCountChanged;
            if (handler != null) {
                handler(this, new ProgressChangedEventArgs(byteCount));
            }
        }

        public event EventHandler<EnvelopeFetchedEventArgs> EnvelopeFetched;

        private void OnSyncProgressChanged(ImapEnvelope envelope) {
            var handler = EnvelopeFetched;
            if (handler != null) {
                handler(this, new EnvelopeFetchedEventArgs(envelope));
            }
        }


        /// <summary>
        ///     The SEARCH command searches the mailbox for messages that match
        ///     the given searching criteria.  Searching criteria consist of one
        ///     or more search keys.
        /// </summary>
        /// <param name="criteria"> The search criteria. </param>
        /// <returns> The uids of messages matching the given criteria. </returns>
        public async Task<IList<long>> SearchAsync(string criteria) {
            var command = String.Format("UID SEARCH UID {0}", criteria);
            var id = await _connection.WriteCommandAsync(command);
            return await ReadSearchResponseAsync(id);
        }

        public async Task DeleteMailsAsync(IEnumerable<long> uids) {
            var uidString = uids.ToCommaSeparatedValues();
            var command = string.Format(@"UID STORE {0} +FLAGS.SILENT (\Deleted)", uidString);
            var id = await _connection.WriteCommandAsync(command);
            await ReadBasicResponseAsync(id);

            id = await _connection.WriteCommandAsync("EXPUNGE");
            await ReadBasicResponseAsync(id);
        }

        public async Task MarkAsFlaggedAsync(IEnumerable<long> uids) {
            var uidString = uids.ToCommaSeparatedValues();
            var command = string.Format(@"UID STORE {0} +FLAGS.SILENT (\Flagged)", uidString);
            var id = await _connection.WriteCommandAsync(command);
            await ReadBasicResponseAsync(id);
        }

        public async Task MarkAsNotFlaggedAsync(IEnumerable<long> uids) {
            var uidString = uids.ToCommaSeparatedValues();
            var command = string.Format(@"UID STORE {0} -FLAGS.SILENT (\Flagged)", uidString);
            var id = await _connection.WriteCommandAsync(command);
            await ReadBasicResponseAsync(id);
        }

        public async Task MarkAsSeenAsync(IEnumerable<long> uids) {
            var uidString = uids.ToCommaSeparatedValues();
            var command = string.Format(@"UID STORE {0} +FLAGS.SILENT (\Seen)", uidString);
            var id = await _connection.WriteCommandAsync(command);
            await ReadBasicResponseAsync(id);
        }

        public async Task MarkAsNotSeenAsync(IEnumerable<long> uids) {
            var uidString = uids.ToCommaSeparatedValues();
            var command = string.Format(@"UID STORE {0} -FLAGS.SILENT (\Seen)", uidString);
            var id = await _connection.WriteCommandAsync(command);
            await ReadBasicResponseAsync(id);
        }

        public async Task MarkAsNotAnsweredAsync(IEnumerable<long> uids) {
            //muuuuuuuuuuu
            var uidString = uids.ToCommaSeparatedValues();
            var command = string.Format(@"UID STORE {0} +FLAGS.SILENT (\Answered)", uidString);
            var id = await _connection.WriteCommandAsync(command);
            await ReadBasicResponseAsync(id);
        }

        public async Task MarkAsAnsweredAsync(IEnumerable<long> uids) {
            //muuuuuuuuuuu
            var uidString = uids.ToCommaSeparatedValues();
            var command = string.Format(@"UID STORE {0} -FLAGS.SILENT (\Answered)", uidString);
            var id = await _connection.WriteCommandAsync(command);
            await ReadBasicResponseAsync(id);
        }


        public async Task MoveMailsAsync(ICollection<long> uids, string destination) {
            var uidString = uids.ToCommaSeparatedValues();
            if (string.IsNullOrWhiteSpace(destination)) {
                return;
            }

            string command, id;
            var encodedName = EncodeName(destination);
            if (_connection.CanMove) {
                command = string.Format("UID MOVE {0} \"{1}\"", uidString, encodedName);
                id = await _connection.WriteCommandAsync(command);
                await ReadBasicResponseAsync(id);
            } else {
                command = string.Format("UID COPY {0} \"{1}\"", uidString, encodedName);
                id = await _connection.WriteCommandAsync(command);
                await ReadBasicResponseAsync(id);
                await DeleteMailsAsync(uids);
            }
        }

        private async Task ReadBasicResponseAsync(string id) {
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

        /// <summary>
        ///     The IDLE command may be used with any IMAP4 server implementation
        ///     that returns "IDLE" as one of the supported capabilities to the
        ///     CAPABILITY command.  If the server does not advertise the IDLE
        ///     capability, the client MUST NOT use the IDLE command and must poll
        ///     for mailbox updates.
        ///     http://tools.ietf.org/html/rfc2177
        /// </summary>
        public async Task IdleAsync() {
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
                if (string.IsNullOrEmpty(line.Text)) {
                    break;
                }
                OnChangeNotificationReceived();
            }
        }

        public async Task StopIdleAsync() {
            await _connection.WriteCommandAsync(ImapCommands.Done);
            IsIdle = false;
        }

        private async Task<IList<long>> ReadSearchResponseAsync(string commandId) {
            var list = new List<long>();
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
                list.AddRange(from Match match in matches select Int64.Parse(match.Value));
            }

            return list;
        }

        internal static HeaderCollection ParseHeaders(string text) {
            var key = string.Empty;
            var value = string.Empty;

            var headers = new HeaderCollection();
            var reader = new StringReader(text);


            while (true) {
                var line = reader.ReadLine();
                if (line == null) {
                    break;
                }
                if (line.StartsWith(" ")) {
                    value += line;
                    continue;
                }

                if (!string.IsNullOrEmpty(key)) {
                    headers.Add(new KeyValuePair<string, string>(key, value));
                    key = string.Empty;
                    value = string.Empty;
                }

                var items = line.Split(':');
                if (items.Length <= 1)
                    continue;

                key = items[0];
                value = items[1];
            }

            return headers;
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

        public async Task<IDictionary<long, HeaderCollection>> FetchHeadersAsync(IEnumerable<long> uids) {
            var command = string.Format("UID FETCH {0} BODY[HEADER]", uids
                .Select(x => x.ToString(CultureInfo.InvariantCulture))
                .Aggregate((c, n) => c + ',' + n));

            var id = await _connection.WriteCommandAsync(command);
            return await ReadFetchHeaderResponseAsync(id);
        }

        private async Task<IDictionary<long, HeaderCollection>> ReadFetchHeaderResponseAsync(string commandId) {
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

            var headers = new Dictionary<long, HeaderCollection>();

            foreach (var segment in segments) {
                try {
                    var key = long.Parse(UidRegex.Match(segment).Value.Split(' ')[1]);
                    var value = ParseHeaders(segment);
                    headers.Add(key, value);
                } catch (Exception ex) {
                    Debug.WriteLine(ex);
                }
            }

            return headers;
        }

        public async Task<List<ImapEnvelope>> FetchEnvelopesAsync(ICollection<long> uids) {
            var envelopes = new List<ImapEnvelope>();

            var badges = uids.Bundle(500);
            foreach (var command in badges.Select(badge => string.Format("UID FETCH {0} ALL", badge
                .Select(x => x.ToString(CultureInfo.InvariantCulture))
                .Aggregate((c, n) => c + ',' + n)))) {
                var id = await _connection.WriteCommandAsync(command);
                envelopes.AddRange(await ReadFetchEnvelopesResponseAsync(id));
            }

            return envelopes;
        }

        private async Task<IEnumerable<ImapEnvelope>> ReadFetchEnvelopesResponseAsync(string commandId) {
            var lines = new List<ImapResponseLine> { await _connection.ReadAsync() };
            var envelopes = new List<ImapEnvelope>();

            while (true) {
                var line = await _connection.ReadAsync();
                if (line.IsUntagged || line.TerminatesCommand(commandId)) {
                    using (var writer = new StringWriter()) {
                        foreach (var l in lines) {
                            await writer.WriteAsync(l.Text);
                            await writer.WriteLineAsync();
                        }
                        ImapEnvelope envelope = null;
                        try {
                            envelope = ImapEnvelope.Parse(writer.ToString());
                            envelopes.Add(envelope);
                        } catch (Exception ex) {
                            Debug.WriteLine(ex);
                        } finally {
                            OnSyncProgressChanged(envelope);
                        }
                    }
                    lines.Clear();
                }

                if (line.TerminatesCommand(commandId)) {
                    break;
                }

                lines.Add(line);
            }

            return envelopes;
        }

        public async Task<byte[]> FetchMessageBodyAsync(long uid) {
            var command = string.Format("UID FETCH {0} BODY[]", uid);
            var id = await _connection.WriteCommandAsync(command);
            return ReadMessageBodyResponse(id);
        }

        public async Task<byte[]> PeekMessageBodyAsync(long uid) {
            var command = string.Format("UID FETCH {0} BODY.PEEK[]", uid);
            var id = await _connection.WriteCommandAsync(command);
            return ReadMessageBodyResponse(id);
        }

        private byte[] ReadMessageBodyResponse(string id) {
            var byteCount = 0;

            var stream = new MemoryStream();
            OnByteCountChanged(byteCount);
            using (var writer = new BinaryWriter(stream, Encoding.Default, true)) {
                using (
                    var reader = new BinaryReader(_connection.SecureStream, Encoding.Default, true)) {
                    while (true) {
                        var buffer = new MemoryStream();
                        while (true) {
                            var b = reader.ReadByte();
                            buffer.WriteByte(b);

                            // new lines
                            if (b != 13)
                                continue;

                            b = reader.ReadByte();
                            buffer.WriteByte(b);
                            if (b == 10) {
                                break;
                            }
                        }

                        var bytes = buffer.ToArray();
                        var line = new ImapResponseLine(Encoding.ASCII.GetString(bytes));
                        if (line.IsUntagged) {
                            continue;
                        }

                        if (line.TerminatesCommand(id)) {
                            break;
                        }

                        writer.Write(bytes);
                        byteCount += bytes.Length;
                        OnByteCountChanged(byteCount);
                    }
                }


                var trimLength = 0;
                using (var reader = new BinaryReader(stream)) {
                    stream.Seek(0, SeekOrigin.Begin);
                    var buffer = reader.ReadBytes((int)stream.Length);

                    for (var i = buffer.Length - 1; i > 0; i--) {
                        var b = buffer[i];
                        // Detect trailing line feeds (13), carriage returns (10) and the closing bracket (41).
                        if (b == 10 || b == 13 || b == 41) {
                            trimLength++;
                        }

                        if (b == 41) {
                            break;
                        }
                    }

                    var length = buffer.Length - trimLength;
                    var target = new byte[length];
                    Buffer.BlockCopy(buffer, 0, target, 0, length);
                    return target;
                }
            }
        }
    }
}
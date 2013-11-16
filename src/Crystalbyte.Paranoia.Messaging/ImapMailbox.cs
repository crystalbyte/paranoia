﻿#region Using directives

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#endregion

namespace Crystalbyte.Paranoia.Messaging {
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

        /// <summary>
        ///   The SEARCH command searches the mailbox for messages that match
        ///   the given searching criteria.  Searching criteria consist of one
        ///   or more search keys.
        /// </summary>
        /// <param name="criteria"> The search criteria. </param>
        /// <returns> The uids of messages matching the given criteria. </returns>
        public async Task<IList<int>> SearchAsync(string criteria) {
            var command = String.Format("UID SEARCH {0}", criteria);
            var id = await _connection.WriteCommandAsync(command);
            return await ReadSearchResponseAsync(id);
        }

        public async Task DeleteAsync(IEnumerable<long> uids) {
            var command = string.Format(@"UID STORE {0} +FLAGS (\Deleted)", uids.ToCommaSeparatedValues());
            var id = await _connection.WriteCommandAsync(command);
            await ReadStoreResponseAsync(id);
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
            var envelopes = new List<ImapEnvelope>();
            // We currently only support single line responses.
            while (true) {
                var line = await _connection.ReadAsync();
                if (line.TerminatesCommand(commandId)) {
                    break;
                }
                envelopes.Add(ImapEnvelope.Parse(line.Text));
            }

            return envelopes;
        }

        public async Task<string> FetchMessageBodyAsync(long uid) {
            var command = string.Format("UID FETCH {0} BODY[]", uid);
            var id = await _connection.WriteCommandAsync(command);
            return await ReadFetchMessageBodyAsync(id);
        }

        private async Task<string> ReadFetchMessageBodyAsync(string id) {
            using (var writer = new StringWriter()) {
                while (true) {
                    var line = await _connection.ReadAsync();
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
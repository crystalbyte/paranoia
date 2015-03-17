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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace Crystalbyte.Paranoia.Mail {
    internal static class QpConverter {
        private static readonly string SoftLineBreak = Environment.NewLine;

        private static int GetDrift(int remainder) {
            return remainder == 0 ? 1 : 0;
        }

        public static string ToQuotedPrintableString(byte[] bytes) {
            var text = Encoding.UTF8.GetString(bytes);
            using (var reader = new StringReader(text)) {
                using (var writer = new StringWriter()) {
                    while (true) {
                        var line = reader.ReadLine();
                        if (line == null) {
                            break;
                        }

                        line = Regex.Replace(line, RegexPatterns.QuotedPrintableOutOfBounceCharacterPattern,
                            OnEncodingMatchEvaluation);
                        line = Regex.Replace(line, RegexPatterns.QuotedPrintableTrailingWhitespacePattern,
                            OnEncodingMatchEvaluation);

                        var remainder = (line.Length%76);
                        var breaks = (line.Length/76);

                        var blocks = breaks - GetDrift(remainder);
                        for (var i = 0; i < blocks; i++) {
                            var block = line.Substring(0, 75);
                            line = line.Remove(0, 75);

                            var croppedMatch = Regex.Match(block, RegexPatterns.QuotedPrintableCroppedEncodedItemPattern);
                            if (croppedMatch.Success) {
                                line = line.Insert(0, croppedMatch.Value);
                                block = block.Substring(0, block.Length - croppedMatch.Value.Length);
                            }

                            writer.Write(block);
                            writer.Write('=');
                            writer.Write(Environment.NewLine);
                        }

                        if (reader.Peek() == -1) {
                            writer.Write(line);
                        }
                        else {
                            writer.WriteLine(line);
                        }
                    }

                    return writer.ToString();
                }
            }
        }

        private static string OnEncodingMatchEvaluation(Match match) {
            var replacement = string.Empty;
            var chars = match.Value.ToCharArray();
            foreach (var character in chars) {
                var hex = Convert.ToInt32(character).ToString("X");
                if (hex.Length == 1) {
                    hex = "0" + hex;
                }

                replacement += "=" + hex;
            }
            return replacement;
        }

        public static string FromQuotedPrintable(string text, Encoding targetEncoding) {
            var pattern = RegexPatterns.QuotedPrintableEncodedItemPattern;
            return Regex.Replace(text, pattern, match => {
                                                    var bytes = new List<byte>();
                                                    var values = match.Value.Split(new[] {'='},
                                                        StringSplitOptions.RemoveEmptyEntries);
                {
                    bytes.AddRange(from value in values
                        where value != SoftLineBreak
                        select Convert.ToInt32(value, 16)
                        into int32
                        select (byte) int32);
                }

                                                    return targetEncoding.GetString(bytes.ToArray());
                                                });
        }
    }
}
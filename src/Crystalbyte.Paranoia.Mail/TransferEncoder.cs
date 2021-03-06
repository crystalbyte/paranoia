﻿#region Copyright Notice & Copying Permission

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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace Crystalbyte.Paranoia.Mail {
    public static class TransferEncoder {
        public static string Encode(string text, string transferEncoding, string charset, bool useBockText = true) {
            switch (transferEncoding) {
                case ContentTransferEncodings.QuotedPrintable: {
                    var bytes = Encoding.UTF8.GetBytes(text);
                    return QpConverter.ToQuotedPrintableString(bytes);
                }
                case ContentTransferEncodings.None: {
                    return text;
                }
                default: {
                    var bytes = Encoding.UTF8.GetBytes(text);
                    var encodedText = Convert.ToBase64String(bytes);
                    return useBockText ? encodedText.ToBlockText(76) : encodedText;
                }
            }
        }

        public static string Decode(string literals, string transferEncoding, string charset) {
            Encoding targetEncoding;

            var value = charset.ToLower();
            switch (value) {
                case "cp1252":
                    // Cp1252 is not recognized under this name
                    charset = Charsets.Windows1252;
                    break;
                case "x-unknown":
                    charset = Charsets.Utf8;
                    break;
            }

            try {
                // if this goes haywire
                targetEncoding = Encoding.GetEncoding(charset);
            }
            catch (ArgumentException) {
                // try this one
                targetEncoding = Encoding.UTF8;
            }

            switch (transferEncoding.ToLower()) {
                case ContentTransferEncodings.QuotedPrintable: {
                    return QpConverter.FromQuotedPrintable(literals, targetEncoding);
                }
                case ContentTransferEncodings.Base64: {
                    var bytes = Convert.FromBase64String(literals);
                    return targetEncoding.GetString(bytes);
                }
                default: {
                    // no encoding
                    return literals;
                }
            }
        }

        public static string Decode(string text) {
            const string pattern = @"=\?[-A-Za-z0-9]+\?[QBqb]\?[.\u0020-\u003E\u0040-\u007E]+\?=";
            text = Regex.Replace(text, pattern, x => DecodeHeaderBlock(x.Value));

            // change _ to 'space'
            // http://www.faqs.org/rfcs/rfc1342.html
            text = text.Replace('_', ' ');
            return text;
        }

        private static string DecodeHeaderBlock(string text) {
            text = text.Trim('=').Trim('?');
            var split = text.Split('?');
            Debug.Assert(split.Length == 3);

            var charset = split[0];
            var encoding = split[1].ToUpper();
            var message = split[2];

            string decodedText;

            switch (encoding) {
                case "Q": {
                    decodedText = Decode(message, ContentTransferEncodings.QuotedPrintable, charset);
                    break;
                }
                default: {
                    decodedText = Decode(message, ContentTransferEncodings.Base64, charset);
                    break;
                }
            }

            return decodedText;
        }

        public static string Encode(string text, HeaderEncodingTypes headerEncoding) {
            var chars = text.ToCharArray();
            var needsEncoding = chars.Any(character => character > 128);

            if (!needsEncoding) {
                return text;
            }

            switch (headerEncoding) {
                case HeaderEncodingTypes.Base64:
                    return EncodeHeaderBase64(chars);
                default:
                    return EncodeHeaderQuotedPrintable(chars);
            }
        }

        private static string EncodeHeaderQuotedPrintable(char[] chars) {
            var text = new string(chars);
            var message = Encode(text, ContentTransferEncodings.QuotedPrintable, Charsets.Utf8);
            return string.Format("=?{0}?Q?{1}?=", Charsets.Utf8, message);
        }

        private static string EncodeHeaderBase64(char[] chars) {
            var text = new string(chars);
            var message = Encode(text, ContentTransferEncodings.Base64, Charsets.Utf8, false);
            return string.Format("=?{0}?B?{1}?=", Charsets.Utf8, message);
        }
    }
}
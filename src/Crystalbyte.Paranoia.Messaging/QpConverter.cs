#region Microsoft Public License (Ms-PL)

// // Microsoft Public License (Ms-PL)
// // 
// // This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.
// // 
// // 1. Definitions
// // 
// // The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
// // 
// // A "contribution" is the original software, or any additions or changes to the software.
// // 
// // A "contributor" is any person that distributes its contribution under this license.
// // 
// // "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// // 
// // 2. Grant of Rights
// // 
// // (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// // 
// // (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// // 
// // 3. Conditions and Limitations
// // 
// // (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// // 
// // (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
// // 
// // (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
// // 
// // (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
// // 
// // (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Crystalbyte.Paranoia.Messaging {
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

                        line = Regex.Replace(line, RegexPatterns.QuotedPrintableOutOfBounceCharacterPattern, OnEncodingMatchEvaluation);
                        line = Regex.Replace(line, RegexPatterns.QuotedPrintableTrailingWhitespacePattern, OnEncodingMatchEvaluation);

                        var remainder = (line.Length % 76);
                        var breaks = (line.Length / 76);

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
                        } else {
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
                                                        var values = match.Value.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                                                        {
                                                            bytes.AddRange(from value in values where value != SoftLineBreak select Convert.ToInt32(value, 16) into int32 select (byte)int32);
                                                        }

                                                        return targetEncoding.GetString(bytes.ToArray());
                                                    });
        }
    }
}
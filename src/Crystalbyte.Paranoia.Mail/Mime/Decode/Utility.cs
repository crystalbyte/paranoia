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

#endregion

namespace Crystalbyte.Paranoia.Mail.Mime.Decode {
    /// <summary>
    ///     Contains common operations needed while decoding.
    /// </summary>
    internal static class Utility {
        /// <summary>
        ///     Remove quotes, if found, around the string.
        /// </summary>
        /// <param name="text"> Text with quotes or without quotes </param>
        /// <returns> Text without quotes </returns>
        /// <exception cref="ArgumentNullException">
        ///     If
        ///     <paramref name="text" />
        ///     is
        ///     <see langword="null" />
        /// </exception>
        public static string RemoveQuotesIfAny(string text) {
            if (text == null)
                throw new ArgumentNullException("text");

            // Check if there are quotes at both ends and have at least two characters
            if (text.Length > 1 && text[0] == '"' && text[text.Length - 1] == '"') {
                // Remove quotes at both ends
                return text.Substring(1, text.Length - 2);
            }

            // If no quotes were found, the text is just returned
            return text;
        }

        /// <summary>
        ///     Split a string into a list of strings using a specified character.<br />
        ///     Everything inside quotes are ignored.
        /// </summary>
        /// <param name="input"> A string to split </param>
        /// <param name="toSplitAt"> The character to use to split with </param>
        /// <returns> A List of strings that was delimited by the <paramref name="toSplitAt" /> character </returns>
        public static List<string> SplitStringWithCharNotInsideQuotes(string input, char toSplitAt) {
            var elements = new List<string>();

            var lastSplitLocation = 0;
            var insideQuote = false;

            var characters = input.ToCharArray();

            for (var i = 0; i < characters.Length; i++) {
                var character = characters[i];
                if (character == '\"')
                    insideQuote = !insideQuote;

                // Only split if we are not inside quotes
                if (character == toSplitAt && !insideQuote) {
                    // We need to split
                    var length = i - lastSplitLocation;
                    elements.Add(input.Substring(lastSplitLocation, length));

                    // Update last split location
                    // + 1 so that we do not include the character used to split with next time
                    lastSplitLocation = i + 1;
                }
            }

            // Add the last part
            elements.Add(input.Substring(lastSplitLocation, input.Length - lastSplitLocation));

            return elements;
        }
    }
}
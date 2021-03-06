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
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Crystalbyte.Paranoia.Mail.Common.Logging;

#endregion

namespace Crystalbyte.Paranoia.Mail.Mime.Decode {
    /// <summary>
    ///     Utility class for dealing with Base64 encoded strings
    /// </summary>
    internal static class Base64 {
        /// <summary>
        ///     Decodes a base64 encoded string into the bytes it describes
        /// </summary>
        /// <param name="base64Encoded"> The string to decode </param>
        /// <returns> A byte array that the base64 string described </returns>
        public static byte[] Decode(string base64Encoded) {
            // According to http://www.tribridge.com/blog/crm/blogs/brandon-kelly/2011-04-29/Solving-OutOfMemoryException-errors-when-attempting-to-attach-large-Base64-encoded-content-into-CRM-annotations.aspx
            // System.Convert.ToBase64String may leak a lot of memory
            // An OpenPop user reported that OutOfMemoryExceptions were thrown, and supplied the following
            // code for the fix. This should not have memory leaks.
            // The code is nearly identical to the example on MSDN:
            // http://msdn.microsoft.com/en-us/library/system.security.cryptography.frombase64transform.aspx#exampleToggle
            try {
                using (var memoryStream = new MemoryStream()) {
                    base64Encoded = base64Encoded.Replace("\r\n", "");

                    var inputBytes = Encoding.ASCII.GetBytes(base64Encoded);

                    using (var transform = new FromBase64Transform(FromBase64TransformMode.DoNotIgnoreWhiteSpaces)) {
                        var outputBytes = new byte[transform.OutputBlockSize];

                        // Transform the data in chunks the size of InputBlockSize.
                        const int inputBlockSize = 4;
                        var currentOffset = 0;
                        while (inputBytes.Length - currentOffset > inputBlockSize) {
                            transform.TransformBlock(inputBytes, currentOffset, inputBlockSize, outputBytes, 0);
                            currentOffset += inputBlockSize;
                            memoryStream.Write(outputBytes, 0, transform.OutputBlockSize);
                        }

                        // Transform the final block of data.
                        outputBytes = transform.TransformFinalBlock(inputBytes, currentOffset,
                            inputBytes.Length - currentOffset);
                        memoryStream.Write(outputBytes, 0, outputBytes.Length);
                    }

                    return memoryStream.ToArray();
                }
            }
            catch (FormatException e) {
                DefaultLogger.Log.LogError("Base64: (FormatException) " + e.Message + "\r\nOn string: " + base64Encoded);
                throw;
            }
        }

        /// <summary>
        ///     Decodes a Base64 encoded string using a specified <see cref="System.Text.Encoding" />
        /// </summary>
        /// <param name="base64Encoded"> Source string to decode </param>
        /// <param name="encoding"> The encoding to use for the decoded byte array that <paramref name="base64Encoded" /> describes </param>
        /// <returns> A decoded string </returns>
        /// <exception cref="ArgumentNullException">
        ///     If
        ///     <paramref name="base64Encoded" />
        ///     or
        ///     <paramref name="encoding" />
        ///     is
        ///     <see langword="null" />
        /// </exception>
        /// <exception cref="FormatException">
        ///     If
        ///     <paramref name="base64Encoded" />
        ///     is not a valid base64 encoded string
        /// </exception>
        public static string Decode(string base64Encoded, Encoding encoding) {
            if (base64Encoded == null)
                throw new ArgumentNullException("base64Encoded");

            if (encoding == null)
                throw new ArgumentNullException("encoding");

            return encoding.GetString(Decode(base64Encoded));
        }
    }
}
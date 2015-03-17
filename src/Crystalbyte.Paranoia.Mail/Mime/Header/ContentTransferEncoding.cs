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

#endregion

namespace Crystalbyte.Paranoia.Mail.Mime.Header {
    /// <summary>
    ///     <see cref="Enum" /> that describes the ContentTransferEncoding header field
    /// </summary>
    /// <remarks>
    ///     See <a href="http://tools.ietf.org/html/rfc2045#section-6">RFC 2045 section 6</a> for more details
    /// </remarks>
    public enum ContentTransferEncoding {
        /// <summary>
        ///     7 bit Encoding
        /// </summary>
        SevenBit,

        /// <summary>
        ///     8 bit Encoding
        /// </summary>
        EightBit,

        /// <summary>
        ///     Quoted Printable Encoding
        /// </summary>
        QuotedPrintable,

        /// <summary>
        ///     Base64 Encoding
        /// </summary>
        Base64,

        /// <summary>
        ///     Binary Encoding
        /// </summary>
        Binary
    }
}
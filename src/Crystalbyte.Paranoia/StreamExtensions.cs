#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia
// 
// Crystalbyte.Paranoia is free software: you can redistribute it and/or modify
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

using System.IO;
using System.Threading.Tasks;

#endregion

namespace Crystalbyte.Paranoia {
    public static class StreamExtensions {
        public static async Task<string> ToUtf8StringAsync(this Stream stream) {
            if (stream.CanSeek) {
                stream.Seek(0, SeekOrigin.Begin);
            }

            using (var reader = new StreamReader(stream)) {
                return await reader.ReadToEndAsync();
            }
        }

        public static string ToUtf8String(this Stream stream) {
            if (stream.CanSeek) {
                stream.Seek(0, SeekOrigin.Begin);
            }

            using (var reader = new StreamReader(stream)) {
                return reader.ReadToEnd();
            }
        }
    }
}
#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia.Tests
// 
// Crystalbyte.Paranoia.Tests is free software: you can redistribute it and/or modify
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
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Crystalbyte.Paranoia.Tests.Data {
    [TestClass]
    public sealed class EncodingTests {
        [TestMethod]
        public void Latin1ByteIntegrityTest() {
            var expected = Enumerable.Range(0, 255).Select(x => (byte) x).ToArray();
            var random = new Random();
            random.NextBytes(expected);


            byte[] actual;
            var encoding = Encoding.GetEncoding("iso-8859-1");
            var stream = new MemoryStream(expected);
            using (var reader = new StreamReader(stream, encoding)) {
                var text = reader.ReadToEnd();
                actual = encoding.GetBytes(text);
            }

            var equals = expected.SequenceEqual(actual);
            Assert.IsTrue(equals);
        }
    }
}
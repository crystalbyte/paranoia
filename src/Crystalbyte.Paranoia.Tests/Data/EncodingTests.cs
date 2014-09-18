using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Crystalbyte.Paranoia.Cryptography.Tests {
    [TestClass]
    public sealed class SodiumTests {
        [TestMethod]
        public void InitTest() {
            Sodium.InitNativeLibrary();

            var actual = Sodium.Version;
            const string expected = "0.6.0";
            Assert.AreEqual(expected, actual);
        }
    }
}

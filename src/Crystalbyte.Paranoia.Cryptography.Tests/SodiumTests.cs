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

        [TestMethod]
        public void TestKeyGeneration()
        {
            Sodium.InitNativeLibrary();
            var pkc1 = new PublicKeyCrypto();

            pkc1.GenerateKeyPair();

            Assert.IsFalse(String.IsNullOrEmpty(Encoding.UTF8.GetString(pkc1.PublicKey)));
            Assert.IsFalse(String.IsNullOrEmpty(Encoding.UTF8.GetString(pkc1.PrivateKey)));
        }

        [TestMethod]
        public void TestPublicKeyEncrypt()
        {
            Sodium.InitNativeLibrary();
            var pkc1 = new PublicKeyCrypto();

            pkc1.GenerateKeyPair();

            var message = "TESTTEXT";
            var nonce = PublicKeyCrypto.GenerateNonce();

            var encMessage = pkc1.PublicKeyEncrypt(message, pkc1.PublicKey, nonce);
            var decryptedMessage = pkc1.PrivateKeyDecrypt(encMessage, pkc1.PublicKey, nonce);
            Assert.Equals(message, decryptedMessage);
        }
    }
}

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

            Assert.IsFalse(String.IsNullOrEmpty(Encoding.UTF8.GetString(pkc1.PublicKey)));
            Assert.IsFalse(String.IsNullOrEmpty(Encoding.UTF8.GetString(pkc1.PrivateKey)));
        }

        [TestMethod]
        public void TestPublicKeyEncrypt()
        {
            Sodium.InitNativeLibrary();
            var pkc1 = new PublicKeyCrypto();
            var pkc2 = new PublicKeyCrypto();

            var message = "test";
            var nonce = PublicKeyCrypto.GenerateNonce();

            var encMessage = pkc1.EncryptWithPublicKey(Encoding.ASCII.GetBytes(message), pkc2.PublicKey, nonce);

            var decryptedMessage = pkc2.DecryptWithPrivateKey(encMessage, pkc1.PublicKey, nonce);
            Assert.IsTrue(String.Compare(message, Encoding.ASCII.GetString(decryptedMessage)) == 0);
        }

        [TestMethod]
        public void TestSecretKeyEncrpytion()
        {
            Sodium.InitNativeLibrary();
            var skc = new SecretKeyCrypto();

            var message = "test";
            var cipherText = skc.Encrypt(Encoding.ASCII.GetBytes(message));
            var decryptedMessage = skc.Decrypt(cipherText,skc.Nonce,skc.Key);
            Assert.IsTrue(String.Compare(message, Encoding.ASCII.GetString(decryptedMessage)) == 0);
        }
    }
}

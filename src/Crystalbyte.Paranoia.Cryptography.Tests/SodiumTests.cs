#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia.Cryptography.Tests
// 
// Crystalbyte.Paranoia.Cryptography.Tests is free software: you can redistribute it and/or modify
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
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

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
        public void TestKeyGeneration() {
            Sodium.InitNativeLibrary();
            var pkc1 = new PublicKeyCrypto();

            Assert.IsFalse(String.IsNullOrEmpty(Encoding.UTF8.GetString(pkc1.PublicKey)));
            Assert.IsFalse(String.IsNullOrEmpty(Encoding.UTF8.GetString(pkc1.PrivateKey)));
        }

        [TestMethod]
        public void TestPublicKeyEncrypt() {
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
        public void TestSecretKeyEncrpytion() {
            Sodium.InitNativeLibrary();
            var skc = new SecretKeyCrypto();

            var message = "test";
            var cipherText = skc.Encrypt(Encoding.ASCII.GetBytes(message));
            var decryptedMessage = skc.Decrypt(cipherText, skc.Nonce, skc.Key);
            Assert.IsTrue(String.Compare(message, Encoding.ASCII.GetString(decryptedMessage)) == 0);
        }
    }
}
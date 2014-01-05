using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Crystalbyte.Paranoia.Cryptography.Tests
{
    [TestClass]
    public class RsaEncryptionTest
    {
        [TestMethod]
        public void CreateNewRsa()
        {
            var rsa = new RsaEncryption();
            rsa.GenerateRSA();
            var a = "bal";
        }
    }
}

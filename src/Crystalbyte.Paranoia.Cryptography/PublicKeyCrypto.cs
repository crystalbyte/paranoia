#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia.Cryptography
// 
// Crystalbyte.Paranoia.Cryptography is free software: you can redistribute it and/or modify
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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

#endregion

namespace Crystalbyte.Paranoia.Cryptography {
    public class PublicKeyCrypto : NativeResource {
        private IntPtr _publicKeyPtr;
        private IntPtr _privateKeyPtr;

        private static readonly int KeySize = Marshal.SizeOf<Byte>()*32;
        private static readonly uint UnonceSize = Convert.ToUInt32(Marshal.SizeOf<Byte>()*24);

        // ReSharper disable once InconsistentNaming
        private static readonly int _nonceSize = Marshal.SizeOf<Byte>()*24;
        private static readonly int BoxZeroByteSize = Marshal.SizeOf<Byte>()*16;
        private static readonly int ZeroByteSize = Marshal.SizeOf<Byte>()*32;
        private static readonly int MacBytesSize = ZeroByteSize - BoxZeroByteSize;

        public PublicKeyCrypto() {
            Alloc();
            var err = SafeNativeMethods.CryptoBoxKeypair(_publicKeyPtr, _privateKeyPtr);
            if (err == -1) {
                throw new Exception();
            }
        }

        public PublicKeyCrypto(byte[] publicKey, byte[] privateKey) {
            Alloc();
            WriteKey(publicKey, _publicKeyPtr);
            WriteKey(privateKey, _privateKeyPtr);
        }

        /// <summary>
        ///     Gets the size of the nonce byte array.
        /// </summary>
        public static int NonceSize {
            get { return Convert.ToInt32(_nonceSize/Marshal.SizeOf<Byte>()); }
        }

        //public async Task InitFromFileAsync(string publicKeyPath, string privateKeyPath, string password = "") {

        //    string publicKey = string.Empty;
        //    string privateKey = string.Empty;

        //    await Task.Factory.StartNew(() => {
        //        publicKey = File.ReadAllText(publicKeyPath);
        //        privateKey = File.ReadAllText(privateKeyPath);
        //    });

        //    Alloc();

        //    var decodedPublicKey = Convert.FromBase64String(publicKey);
        //    WriteKey(decodedPublicKey, _publicKeyPtr);


        //    var decodedPrivateKey = Convert.FromBase64String(privateKey);
        //    WriteKey(decodedPrivateKey, _privateKeyPtr);
        //}

        private void WriteKey(byte[] key, IntPtr ptr) {
            Marshal.Copy(key, 0, ptr, KeySize);
        }

        public byte[] PublicKey {
            get {
                var bytes = new byte[KeySize];
                Marshal.Copy(_publicKeyPtr, bytes, 0, KeySize);
                return bytes;
            }
        }

        public byte[] PrivateKey {
            get {
                var bytes = new byte[KeySize];
                Marshal.Copy(_privateKeyPtr, bytes, 0, KeySize);
                return bytes;
            }
        }

        public async Task SavePublicKey(string path) {
            using (var fs = File.OpenWrite(path)) {
                var base64 = Convert.ToBase64String(PublicKey);
                var bytes = Encoding.UTF8.GetBytes(base64);
                await fs.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        public async Task SavePrivateKeyAsync(string path) {
            using (var fs = File.OpenWrite(path)) {
                var base64 = Convert.ToBase64String(PrivateKey);
                var bytes = Encoding.UTF8.GetBytes(base64);
                await fs.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        private void Alloc() {
            _publicKeyPtr = Marshal.AllocHGlobal(KeySize);
            _privateKeyPtr = Marshal.AllocHGlobal(KeySize);
        }

        public static byte[] GenerateNonce() {
            var noncePtr = Marshal.AllocHGlobal(_nonceSize);
            SafeNativeMethods.SodiumMemZero(noncePtr, UnonceSize);
            SafeNativeMethods.RandomBytesBuf(noncePtr, UnonceSize);
            var nonceArray = new byte[_nonceSize];
            Marshal.Copy(noncePtr, nonceArray, 0, _nonceSize);
            Marshal.FreeHGlobal(noncePtr);
            return nonceArray;
        }

        public byte[] EncryptWithPublicKey(byte[] message, byte[] pkey, byte[] nonce) {
            var publicKeyPtr = Marshal.AllocHGlobal(pkey.Length);
            var noncePtr = Marshal.AllocHGlobal(nonce.Length);

            Marshal.Copy(pkey, 0, publicKeyPtr, pkey.Length);
            Marshal.Copy(nonce, 0, noncePtr, nonce.Length);

            var cipherTextPtr = Marshal.AllocHGlobal(message.Length + MacBytesSize);
            var chiperText = new byte[message.Length + MacBytesSize];

            //SafeNativeMethods.SodiumMemZero(cipherTextPtr, Convert.ToUInt32(messageSize + macBytesSize));

            var messagePtr = Marshal.AllocHGlobal(message.Length);
            Marshal.Copy(message, 0, messagePtr, message.Length);

            var err = SafeNativeMethods.CryptoBoxEasy(
                cipherTextPtr, messagePtr, message.Length,
                noncePtr, publicKeyPtr, _privateKeyPtr);

            if (err != 0) {
                throw new Exception();
            }

            Marshal.Copy(cipherTextPtr, chiperText, 0, message.Length + MacBytesSize);

            Marshal.FreeHGlobal(noncePtr);
            Marshal.FreeHGlobal(cipherTextPtr);
            Marshal.FreeHGlobal(messagePtr);
            Marshal.FreeHGlobal(publicKeyPtr);

            return chiperText;
        }

        public byte[] DecryptWithPrivateKey(byte[] cipherText, byte[] pkey, byte[] nonce) {
            var publicKeyPtr = Marshal.AllocHGlobal(pkey.Length);
            var noncePtr = Marshal.AllocHGlobal(nonce.Length);

            Marshal.Copy(pkey, 0, publicKeyPtr, pkey.Length);
            Marshal.Copy(nonce, 0, noncePtr, nonce.Length);

            var messagePtr = Marshal.AllocHGlobal(cipherText.Length - MacBytesSize);
            var message = new byte[cipherText.Length - MacBytesSize];

            var cipherTextPtr = Marshal.AllocHGlobal(cipherText.Length);
            Marshal.Copy(cipherText, 0, cipherTextPtr, cipherText.Length);


            var err = SafeNativeMethods.CryptoBoxOpenEasy(
                messagePtr, cipherTextPtr, cipherText.Length,
                noncePtr, publicKeyPtr, _privateKeyPtr);

            if (err != 0) {
                throw new MessageDecryptionFailedException();
            }

            Marshal.Copy(messagePtr, message, 0, message.Length);

            Marshal.FreeHGlobal(noncePtr);
            Marshal.FreeHGlobal(cipherTextPtr);
            Marshal.FreeHGlobal(messagePtr);
            Marshal.FreeHGlobal(publicKeyPtr);
            return message;
        }

        protected override void DisposeNative() {
            if (_publicKeyPtr != IntPtr.Zero) {
                Marshal.FreeHGlobal(_publicKeyPtr);
            }

            if (_privateKeyPtr != IntPtr.Zero) {
                Marshal.FreeHGlobal(_privateKeyPtr);
            }

            base.DisposeNative();
        }

        private static class SafeNativeMethods {
            [DllImport(Library.Sodium, EntryPoint = "crypto_box_keypair", CallingConvention = CallingConvention.Cdecl)]
            public static extern int CryptoBoxKeypair(IntPtr pk, IntPtr sk);

            [DllImport(Library.Sodium, EntryPoint = "randombytes_buf", CallingConvention = CallingConvention.Cdecl)]
            public static extern void RandomBytesBuf(IntPtr buf, uint size);

            [DllImport(Library.Sodium, EntryPoint = "sodium_memzero", CallingConvention = CallingConvention.Cdecl)]
            public static extern void SodiumMemZero(IntPtr buf, uint size);

            [DllImport(Library.Sodium, EntryPoint = "crypto_box_easy", CallingConvention = CallingConvention.Cdecl)]
            public static extern int CryptoBoxEasy(IntPtr c, IntPtr m, long mlen, IntPtr n, IntPtr pk, IntPtr sk);

            [DllImport(Library.Sodium, EntryPoint = "crypto_box_open_easy", CallingConvention = CallingConvention.Cdecl)
            ]
            public static extern int CryptoBoxOpenEasy(IntPtr m, IntPtr c, long clen, IntPtr n, IntPtr pk, IntPtr sk);
        }
    }
}
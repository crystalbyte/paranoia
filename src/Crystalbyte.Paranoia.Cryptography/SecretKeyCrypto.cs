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
using System.Runtime.InteropServices;

#endregion

namespace Crystalbyte.Paranoia.Cryptography {
    internal class SecretKeyCrypto : NativeResource {
        private IntPtr _noncePtr;
        private IntPtr _keyPtr;

        private byte[] _nonce;
        private byte[] _key;

        private static readonly uint ukeySize = Convert.ToUInt32(Marshal.SizeOf<Byte>()*32);
        private static readonly int keySize = Marshal.SizeOf<Byte>()*32;

        private static readonly uint unonceSize = Convert.ToUInt32(Marshal.SizeOf<Byte>()*24);
        private static readonly int nonceSize = Marshal.SizeOf<Byte>()*24;

        private static readonly uint uboxZeroByteSize = Convert.ToUInt32(Marshal.SizeOf<Byte>()*16);
        private static readonly int boxZeroByteSize = Marshal.SizeOf<Byte>()*16;

        private static readonly uint uzeroByteSize = Convert.ToUInt32(Marshal.SizeOf<Byte>()*32);
        private static readonly int zeroByteSize = Marshal.SizeOf<Byte>()*32;

        private static readonly uint umacBytesSize = uzeroByteSize - uboxZeroByteSize;
        private static readonly int macBytesSize = zeroByteSize - boxZeroByteSize;

        internal byte[] Nonce {
            get { return _nonce; }
        }

        internal byte[] Key {
            get { return _key; }
        }

        public byte[] Encrypt(byte[] message) {
            _noncePtr = Marshal.AllocHGlobal(nonceSize);
            _nonce = new byte[nonceSize];
            SafeNativeMethods.RandomBytesBuf(_noncePtr, unonceSize);
            Marshal.Copy(_noncePtr, _nonce, 0, nonceSize);

            _keyPtr = Marshal.AllocHGlobal(keySize);
            _key = new byte[keySize];
            SafeNativeMethods.RandomBytesBuf(_keyPtr, ukeySize);
            Marshal.Copy(_keyPtr, _key, 0, keySize);

            var messagePtr = Marshal.AllocHGlobal(message.Length);
            Marshal.Copy(message, 0, messagePtr, message.Length);

            var cipherTextPtr = Marshal.AllocHGlobal(message.Length + macBytesSize);
            var cipherText = new byte[message.Length + macBytesSize];

            var err = SafeNativeMethods.CryptoSecretboxEasy(cipherTextPtr, messagePtr, message.Length, _noncePtr,
                _keyPtr);
            if (err != 0) {
                throw new Exception();
            }
            Marshal.Copy(cipherTextPtr, cipherText, 0, message.Length + macBytesSize);

            Marshal.FreeHGlobal(messagePtr);
            Marshal.FreeHGlobal(cipherTextPtr);

            return cipherText;
        }

        public byte[] Decrypt(byte[] cipherText, byte[] nonce, byte[] key) {
            _noncePtr = Marshal.AllocHGlobal(nonceSize);
            _keyPtr = Marshal.AllocHGlobal(keySize);
            Marshal.Copy(nonce, 0, _noncePtr, nonceSize);
            Marshal.Copy(key, 0, _keyPtr, keySize);

            _nonce = nonce;
            _key = key;

            var cipherTextPtr = Marshal.AllocHGlobal(cipherText.Length);
            Marshal.Copy(cipherText, 0, cipherTextPtr, cipherText.Length);

            var decryptedPtr = Marshal.AllocHGlobal(cipherText.Length - macBytesSize);
            var decrypted = new byte[cipherText.Length - macBytesSize];

            var err = SafeNativeMethods.CryptoSecretboxOpenEasy(decryptedPtr, cipherTextPtr, cipherText.Length,
                _noncePtr, _keyPtr);
            if (err != 0) {
                throw new Exception();
            }
            Marshal.Copy(decryptedPtr, decrypted, 0, cipherText.Length - macBytesSize);

            Marshal.FreeHGlobal(cipherTextPtr);
            Marshal.FreeHGlobal(decryptedPtr);
            return decrypted;
        }

        protected override void DisposeNative() {
            Marshal.FreeHGlobal(_noncePtr);
            Marshal.FreeHGlobal(_keyPtr);
            base.DisposeNative();
        }

        private static class SafeNativeMethods {
            [DllImport(Library.Sodium, EntryPoint = "randombytes_buf", CallingConvention = CallingConvention.Cdecl)]
            public static extern void RandomBytesBuf(IntPtr buf, uint size);

            [DllImport(Library.Sodium, EntryPoint = "sodium_memzero", CallingConvention = CallingConvention.Cdecl)]
            public static extern void SodiumMemZero(IntPtr buf, uint size);

            [DllImport(Library.Sodium, EntryPoint = "crypto_secretbox_easy", CallingConvention = CallingConvention.Cdecl
                )]
            public static extern int CryptoSecretboxEasy(IntPtr c, IntPtr m, long mlen, IntPtr n, IntPtr k);

            [DllImport(Library.Sodium, EntryPoint = "crypto_secretbox_open_easy",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern int CryptoSecretboxOpenEasy(IntPtr m, IntPtr c, long clen, IntPtr n, IntPtr k);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Cryptography
{
    public class PublicKeyCrypto : NativeResource
    {
        private byte[] _publicKey;
        private byte[] _privateKey;
        private IntPtr _publicKeyPtr;
        private IntPtr _privateKeyPtr;

        private static readonly uint ukeySize = Convert.ToUInt32(Marshal.SizeOf<Byte>() * 32);
        private static readonly int keySize = Marshal.SizeOf<Byte>() * 32;

        private static readonly uint unonceSize = Convert.ToUInt32(Marshal.SizeOf<Byte>() * 24);
        private static readonly int nonceSize = Marshal.SizeOf<Byte>() * 24;

        private static readonly uint uboxZeroByteSize = Convert.ToUInt32(Marshal.SizeOf<Byte>() * 16);
        private static readonly int boxZeroByteSize = Marshal.SizeOf<Byte>() * 16;

        private static readonly uint uzeroByteSize = Convert.ToUInt32(Marshal.SizeOf<Byte>() * 32);
        private static readonly int zeroByteSize = Marshal.SizeOf<Byte>() * 32;

        private static readonly uint umacBytesSize = uzeroByteSize - uboxZeroByteSize;
        private static readonly int macBytesSize = zeroByteSize - boxZeroByteSize;



        internal byte[] PublicKey { 
            get {return _publicKey;}
        }

        internal byte[] PrivateKey { 
            get {return _privateKey;}
        }


        public void LoadKeyPair(String password)
        {
            // load pem string from db

        }

        public void GenerateKeyPair() 
        {
            _publicKeyPtr  = Marshal.AllocHGlobal(keySize);
            _privateKeyPtr = Marshal.AllocHGlobal(keySize);

            SafeNativeMethods.SodiumMemZero(_publicKeyPtr, ukeySize);
            SafeNativeMethods.SodiumMemZero(_publicKeyPtr, ukeySize);

            var err = SafeNativeMethods.CryptoBoxKeypair(_publicKeyPtr, _privateKeyPtr);
            if (err == -1)
            {
                throw new Exception();
            }
            _publicKey = new byte[keySize];
            _privateKey = new byte[keySize];
            Marshal.Copy(_publicKeyPtr, _publicKey, 0, keySize);
            Marshal.Copy(_privateKeyPtr, _privateKey, 0, keySize);
        }

        public static byte[] GenerateNonce() 
        {
            var noncePtr = Marshal.AllocHGlobal(nonceSize);
            SafeNativeMethods.SodiumMemZero(noncePtr, unonceSize);
            SafeNativeMethods.RandomBytesBuf(noncePtr, unonceSize);
            var nonceArray = new byte[nonceSize];
            Marshal.Copy(noncePtr, nonceArray, 0, nonceSize);
            Marshal.FreeHGlobal(noncePtr);
            return nonceArray;
        }

        public string PublicKeyEncrypt(String message, byte[] pkey, byte[] nonce)
        {
            var messageSize = Encoding.UTF8.GetByteCount(message);

            var publicKeyPtr = Marshal.AllocHGlobal(pkey.Length);
            var noncePtr = Marshal.AllocHGlobal(nonce.Length);

            Marshal.Copy(pkey, 0, publicKeyPtr, pkey.Length);
            Marshal.Copy(nonce, 0, noncePtr, nonce.Length);

            var cipherTextPtr = Marshal.AllocHGlobal(messageSize + macBytesSize);
            SafeNativeMethods.SodiumMemZero(cipherTextPtr, Convert.ToUInt32(messageSize + macBytesSize));

            var messagePtr = Marshal.StringToHGlobalAnsi(message);

            var err = SafeNativeMethods.CryptoBoxEasy(
                cipherTextPtr, messagePtr, Marshal.SizeOf<Byte>()*messageSize,
                noncePtr, publicKeyPtr, _privateKeyPtr);

            if (err != 0)
            {
                throw new Exception();
            }

            var encryptedMessage = Marshal.PtrToStringAnsi(cipherTextPtr);
                
            Marshal.FreeHGlobal(noncePtr);
            Marshal.FreeHGlobal(cipherTextPtr);
            Marshal.FreeHGlobal(messagePtr);
            Marshal.FreeHGlobal(publicKeyPtr);
            return encryptedMessage;
        }

        public string PrivateKeyDecrypt(String encryptedMessage, byte[] pkey, byte[] nonce)
        {
            var messageSize = 1024;

            var encryptedMessageSize = Encoding.UTF8.GetByteCount(encryptedMessage);

            var publicKeyPtr = Marshal.AllocHGlobal(pkey.Length);
            var noncePtr = Marshal.AllocHGlobal(nonce.Length);

            Marshal.Copy(pkey, 0, publicKeyPtr, pkey.Length);
            Marshal.Copy(nonce, 0, noncePtr, nonce.Length);

            var decryptedPtr = Marshal.AllocHGlobal(messageSize);
            var encryptedMessagePtr = Marshal.StringToHGlobalAnsi(encryptedMessage);

            var err = SafeNativeMethods.CryptoBoxOpenEasy(
                decryptedPtr, encryptedMessagePtr, Marshal.SizeOf<Byte>() * encryptedMessageSize,
                noncePtr, publicKeyPtr, _privateKeyPtr);

            if (err != 0)
            {
                //TODO: NSA SPIED ON YOU!
                throw new Exception();
            }

            var decryptedMessage = Marshal.PtrToStringAnsi(encryptedMessagePtr);

            Marshal.FreeHGlobal(noncePtr);
            Marshal.FreeHGlobal(encryptedMessagePtr);
            Marshal.FreeHGlobal(decryptedPtr);
            Marshal.FreeHGlobal(publicKeyPtr);
            return decryptedMessage;
        }

        protected override void DisposeNative()
        {
            Marshal.FreeHGlobal(_publicKeyPtr);
            Marshal.FreeHGlobal(_privateKeyPtr);
            base.DisposeNative();
        }

         private static class SafeNativeMethods 
         {
             [DllImport(Library.Sodium, EntryPoint = "crypto_box_keypair")]
             public static extern int CryptoBoxKeypair(IntPtr pk, IntPtr sk);

             [DllImport(Library.Sodium, EntryPoint = "randombytes_buf")]
             public static extern void RandomBytesBuf(IntPtr buf, uint size);

             [DllImport(Library.Sodium, EntryPoint = "sodium_memzero")]
             public static extern void SodiumMemZero(IntPtr buf, uint size);

             [DllImport(Library.Sodium, EntryPoint = "crypto_box_easy")]
             public static extern int CryptoBoxEasy(IntPtr c, IntPtr m, long mlen, IntPtr n, IntPtr pk, IntPtr sk);

             [DllImport(Library.Sodium, EntryPoint = "crypto_box_open_easy")]
             public static extern int CryptoBoxOpenEasy(IntPtr m, IntPtr c, long clen, IntPtr n, IntPtr pk, IntPtr sk);
         }
    }
}

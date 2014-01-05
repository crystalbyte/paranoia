#region Using directives

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

#endregion

namespace Crystalbyte.Paranoia.Cryptography {
    /// <summary>
    ///   Implementation of a .NET wrapper for the OpenSSL RSA encryption functions. 
    ///   http://www.openssl.org/docs/crypto/rsa.html
    /// </summary>
    public sealed class RsaEncryption : NativeResource {
        private readonly IntPtr _handle;

        public RsaEncryption() {
            _handle = NativeMethods.RsaNew();
        }

        public void GenerateRSA()
        {
            var e = NativeMethods.BN_new();
            NativeMethods.BN_set_word(e, 65537);
            NativeMethods.RsaGenerateKeyEx(_handle, 2048, e, IntPtr.Zero);

            var evpkey = NativeMethods.EVP_PKEY_new();
            var bio = NativeMethods.BIO_new();
            var res = NativeMethods.EVP_PKEY_assign_RSA(evpkey, _handle);
            Debug.Assert(res == 1);

            
            //var bufSize = NativeMethods.RSA_public_encrypt(strlen(message), (unsigned char *) message, encrypted, rsa, RSA_PKCS1_PADDING);

            NativeMethods.BN_free(e);
            
        }

        public String Encrypt() {
            return "";
            //RSA_public_encrypt(int flen, IntPtr from, IntPtr to, IntPtr rsa, int padding);
        }

        protected override void DisposeNative() {
            base.DisposeNative();

            if (_handle != IntPtr.Zero) {
                NativeMethods.RsaFree(_handle);
            }
        }

        private static class NativeMethods {
            [DllImport(OpenSsl.Library, EntryPoint = "BIO_new")]
            public static extern IntPtr BIO_new();

            [DllImport(OpenSsl.Library, EntryPoint = "BIO_Free")]
            public static extern void BIO_Free(IntPtr bio);

            [DllImport(OpenSsl.Library, EntryPoint = "EVP_aes_256_cbc")]
            public static extern IntPtr EVP_aes_256_cbc();

            [DllImport(OpenSsl.Library, EntryPoint = "PEM_write_bio_PKCS8PrivateKey")]
            public static extern int PEM_write_bio_PKCS8PrivateKey(IntPtr bp, IntPtr x, IntPtr enc, IntPtr kstr, int klen, IntPtr cb, IntPtr u);

            [DllImport(OpenSsl.Library, EntryPoint = "EVP_PKEY_assign_RSA")]
            public static extern int EVP_PKEY_assign_RSA(IntPtr pkey, IntPtr keypair);

            [DllImport(OpenSsl.Library, EntryPoint = "EVP_PKEY_new")]
            public static extern IntPtr EVP_PKEY_new();

            [DllImport(OpenSsl.Library, EntryPoint = "BN_new")]
            public static extern IntPtr BN_new();

            [DllImport(OpenSsl.Library, EntryPoint = "BN_free")]
            public static extern void BN_free(IntPtr bignum);

            [DllImport(OpenSsl.Library, EntryPoint = "BN_set_word")]
            public static extern void BN_set_word(IntPtr bignum, ulong w);

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_new")]
            public static extern IntPtr RsaNew();

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_free")]
            public static extern void RsaFree(IntPtr handle);

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_generate_key_ex")]
            public static extern IntPtr RsaGenerateKeyEx(IntPtr rsa, int bits, IntPtr e, IntPtr cb);

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_public_encrypt")]
            public static extern int RSA_public_encrypt(int flen, IntPtr from, IntPtr to, IntPtr rsa, int padding);

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_public_decrypt")]
            public static extern int RSA_public_decrypt(int flen, IntPtr from, IntPtr to, IntPtr rsa, int padding);

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_private_encrypt")]
            public static extern int RSA_private_encrypt(int flen, IntPtr from, IntPtr to, IntPtr rsa, int padding);

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_private_decrypt")]
            public static extern int RSA_private_decrypt(int flen, IntPtr from, IntPtr to, IntPtr rsa, int padding);

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_sign")]
            public static extern int RSA_sign(int type, IntPtr m, uint m_len, IntPtr sigret, IntPtr siglen, IntPtr rsa);

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_verify")]
            public static extern int RSA_verify(int type, IntPtr m, uint m_len, IntPtr sigbuf, uint siglen, IntPtr rsa);

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_size")]
            public static extern int RSA_size(IntPtr rsa);

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_check_key")]
            public static extern int RSA_check_key(IntPtr rsa);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BigNum {
            /// <summary>
            ///   Pointer to an array of 'BN_BITS2' bit chunks.
            /// </summary>
            public readonly IntPtr D;

            /// <summary>
            ///   Index of last used d +1.
            /// </summary>
            public readonly int Top;

            /// <summary>
            ///   The next are internal book keeping for bn_expand.
            /// </summary>
            public readonly int DMax;

            public readonly int Neg;
            public readonly int Flags;
        }

        /// <summary>
        ///   Slow generatio
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct BnGenCb {
            /// <summary>
            ///   To handle binary (in)compatibility.
            /// </summary>
            public readonly uint Ver;

            /// <summary>
            ///   Callback specific data;
            /// </summary>
            public readonly IntPtr Arg;

            /// <summary>
            ///   Callback (new style).
            /// </summary>
            public readonly IntPtr Cb;
        }
    }
}
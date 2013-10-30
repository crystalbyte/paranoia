#region Using directives

using System;
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

        //public byte[] GenerateKey() {
        //    //var rsa = Marshal.AllocHGlobal()       
        //}

        protected override void DisposeNative() {
            base.DisposeNative();

            if (_handle != IntPtr.Zero) {
                NativeMethods.RsaFree(_handle);
            }
        }

        private static class NativeMethods {
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
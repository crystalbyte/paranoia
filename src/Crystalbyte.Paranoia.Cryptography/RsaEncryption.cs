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

        protected override void DisposeNative() {
            base.DisposeNative();

            if (_handle != IntPtr.Zero) {
                NativeMethods.RsaFree(_handle);
            }
        }

        private static class NativeMethods {
            [DllImport(OpenSsl.Library, EntryPoint = "RAND_seed")]
            public static extern void RandSeed(IntPtr buf, int num);

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_new")]
            public static extern IntPtr RsaNew();

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_generate_key_ex")]
            public static extern IntPtr RsaGenerateKeyEx(IntPtr rsa, int bits, IntPtr e, IntPtr cb);

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_free")]
            public static extern void RsaFree(IntPtr handle);
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
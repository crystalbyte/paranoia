using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Cryptography {
    /// <summary>
    /// Implementation of a .NET wrapper for the OpenSSL RSA encryption functions. 
    /// http://www.openssl.org/docs/crypto/rsa.html
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
            /// Pointer to an array of 'BN_BITS2' bit chunks.
            /// </summary>
            public IntPtr D;

            /// <summary>
            /// Index of last used d +1.
            /// </summary>
            public int Top;

            /// <summary>
            /// The next are internal book keeping for bn_expand.
            /// </summary>
            public int DMax;
            public int Neg;
            public int Flags;
        }

        /// <summary>
        /// Slow generatio
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct BnGenCb {
            /// <summary>
            /// To handle binary (in)compatibility.
            /// </summary>
            public uint Ver;

            /// <summary>
            /// Callback specific data;
            /// </summary>
            public IntPtr Arg;

            /// <summary>
            /// Callback (new style).
            /// </summary>
            public IntPtr Cb;
        }
    }

    
}

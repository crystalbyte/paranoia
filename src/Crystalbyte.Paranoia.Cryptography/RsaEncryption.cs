using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Cryptography {
    /// <summary>
    /// Implementation of a .NET wrapper for RSA encryption. 
    /// http://www.openssl.org/docs/crypto/rsa.html
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
            [DllImport(OpenSsl.Library, EntryPoint = "RSA_new")]
            public static extern IntPtr RsaNew();

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_free")]
            public static extern void RsaFree(IntPtr handle);
        }
    }

    
}

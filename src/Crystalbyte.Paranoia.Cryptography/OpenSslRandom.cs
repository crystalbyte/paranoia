#region Using directives

using System;
using System.Runtime.InteropServices;

#endregion

namespace Crystalbyte.Paranoia.Cryptography {
    public static class OpenSslRandom {

        public static void Seed(byte[] bytes, int number) {
            var handle = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, handle, bytes.Length);
            NativeMethods.RandSeed(handle, number);
            Marshal.FreeHGlobal(handle);
        }

        public static void AddEntropy(byte[] bytes, int number, double entropy) {
            var handle = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, handle, bytes.Length);
            NativeMethods.RandAdd(handle, number, entropy);
            Marshal.FreeHGlobal(handle);
        }

        public static bool AddEntropyFromEvents(int iMsg, IntPtr wParam, IntPtr lParam) {
            return NativeMethods.RandEvent(iMsg, wParam, lParam) > 0;
        }

        public static bool IsSeededSufficiently {
            get { return NativeMethods.RandStatus() > 0; }
        }

        private static class NativeMethods {
            [DllImport(OpenSsl.Library, EntryPoint = "RAND_seed")]
            public static extern void RandSeed(IntPtr buf, int num);

            [DllImport(OpenSsl.Library, EntryPoint = "RAND_add")]
            public static extern void RandAdd(IntPtr buf, int num, double entropy);

            [DllImport(OpenSsl.Library, EntryPoint = "RAND_status")]
            public static extern int RandStatus();

            [DllImport(OpenSsl.Library, EntryPoint = "RAND_event")]
            public static extern int RandEvent(int iMsg, IntPtr wParam, IntPtr lParam);
        }
    }
}
using System;
using System.IO;
using System.Runtime.InteropServices;
using Crystalbyte.Paranoia.Cryptography.Properties;

namespace Crystalbyte.Paranoia.Cryptography {

    public static class Sodium {

        public static void InitNativeLibrary() {
            var myass = System.Reflection.Assembly.GetExecutingAssembly();
            var info = new FileInfo(myass.Location);
            if (info.Directory == null) {
                throw new DirectoryNotFoundException(Resources.ExecutingDirectoryNotFound);
            }

            var directory = Environment.Is64BitProcess ? "x64" : "x86";
            var path = string.Format(@"{0}\{1}\{2}", info.Directory.FullName, directory, "libsodium.dll");
            NativeMethods.LoadLibraryEx(path, IntPtr.Zero, 0);
            Init();
        }

        public static void Init()
        {
            var err = SafeNativeMethods.Init();
            if (err == -1) {
                throw new Exception();
            }

        }

        public static string Version {
            get {
                var handle = SafeNativeMethods.SodiumVersionString();
                return Marshal.PtrToStringAnsi(handle);
            }
        }

        private static class NativeMethods {
            [DllImport(Library.Kernel32)]
            public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);
        }

        private static class SafeNativeMethods {
            [DllImport(Library.Sodium, EntryPoint = "sodium_version_string")]
            public static extern IntPtr SodiumVersionString();
            [DllImport(Library.Sodium, EntryPoint = "sodium_init")]
            public static extern int Init();
        }
    }
}
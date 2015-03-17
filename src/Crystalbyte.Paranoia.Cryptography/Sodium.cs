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
using System.Reflection;
using System.Runtime.InteropServices;
using Crystalbyte.Paranoia.Cryptography.Properties;

#endregion

namespace Crystalbyte.Paranoia.Cryptography {
    public static class Sodium {
        public static void InitNativeLibrary() {
            var myass = Assembly.GetExecutingAssembly();
            var info = new FileInfo(myass.Location);
            if (info.Directory == null) {
                throw new DirectoryNotFoundException(Resources.ExecutingDirectoryNotFound);
            }

            var directory = Environment.Is64BitProcess ? "x64" : "x86";
            var path = string.Format(@"{0}\{1}\{2}", info.Directory.FullName, directory, "libsodium.dll");
            NativeMethods.LoadLibraryEx(path, IntPtr.Zero, 0);
            Init();
        }

        public static void Init() {
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
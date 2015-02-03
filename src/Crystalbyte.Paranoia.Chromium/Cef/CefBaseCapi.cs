#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefBase {
        public int Size;
        public IntPtr AddRef;
        public IntPtr Release;
        public IntPtr HasOneRef;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefBaseCapiDelegates {
        public delegate void AddRefCallback(IntPtr self);

        public delegate int ReleaseCallback(IntPtr self);

        public delegate int HasOneRefCallback(IntPtr self);
    }
}
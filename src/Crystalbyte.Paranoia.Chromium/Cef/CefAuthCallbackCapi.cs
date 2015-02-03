#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefAuthCallback {
        public CefBase Base;
        public IntPtr Cont;
        public IntPtr Cancel;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefAuthCallbackCapiDelegates {
        public delegate void ContCallback(IntPtr self, IntPtr username, IntPtr password);

        public delegate void CancelCallback(IntPtr self);
    }
}
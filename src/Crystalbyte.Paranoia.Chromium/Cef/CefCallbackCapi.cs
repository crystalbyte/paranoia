#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefCallback {
        public CefBase Base;
        public IntPtr Cont;
        public IntPtr Cancel;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefCompletionCallback {
        public CefBase Base;
        public IntPtr OnComplete;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefCallbackCapiDelegates {
        public delegate void ContCallback2(IntPtr self);

        public delegate void CancelCallback2(IntPtr self);

        public delegate void OnCompleteCallback(IntPtr self);
    }
}
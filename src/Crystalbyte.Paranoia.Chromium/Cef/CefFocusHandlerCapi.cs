#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;
using Crystalbyte.Paranoia.Cef.Internal;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefFocusHandler {
        public CefBase Base;
        public IntPtr OnTakeFocus;
        public IntPtr OnSetFocus;
        public IntPtr OnGotFocus;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefFocusHandlerCapiDelegates {
        public delegate void OnTakeFocusCallback(IntPtr self, IntPtr browser, int next);

        public delegate int OnSetFocusCallback(IntPtr self, IntPtr browser, CefFocusSource source);

        public delegate void OnGotFocusCallback(IntPtr self, IntPtr browser);
    }
}
#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefRequestContextHandler {
        public CefBase Base;
        public IntPtr GetCookieManager;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefRequestContextHandlerCapiDelegates {
        public delegate IntPtr GetCookieManagerCallback(IntPtr self);
    }
}
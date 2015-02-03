#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefGeolocationCallback {
        public CefBase Base;
        public IntPtr Cont;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefGeolocationHandler {
        public CefBase Base;
        public IntPtr OnRequestGeolocationPermission;
        public IntPtr OnCancelGeolocationPermission;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefGeolocationHandlerCapiDelegates {
        public delegate void ContCallback5(IntPtr self, int allow);

        public delegate int OnRequestGeolocationPermissionCallback(
            IntPtr self, IntPtr browser, IntPtr requestingUrl, int requestId, IntPtr callback);

        public delegate void OnCancelGeolocationPermissionCallback(
            IntPtr self, IntPtr browser, IntPtr requestingUrl, int requestId);
    }
}
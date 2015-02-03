#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefResourceHandler {
        public CefBase Base;
        public IntPtr ProcessRequest;
        public IntPtr GetResponseHeaders;
        public IntPtr ReadResponse;
        public IntPtr CanGetCookie;
        public IntPtr CanSetCookie;
        public IntPtr Cancel;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefResourceHandlerCapiDelegates {
        public delegate int ProcessRequestCallback(IntPtr self, IntPtr request, IntPtr callback);

        public delegate void GetResponseHeadersCallback(
            IntPtr self, IntPtr response, ref long responseLength, IntPtr redirecturl);

        public delegate int ReadResponseCallback(
            IntPtr self, IntPtr dataOut, int bytesToRead, ref int bytesRead, IntPtr callback);

        public delegate int CanGetCookieCallback(IntPtr self, IntPtr cookie);

        public delegate int CanSetCookieCallback(IntPtr self, IntPtr cookie);

        public delegate void CancelCallback7(IntPtr self);
    }
}
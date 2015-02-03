#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;
using Crystalbyte.Paranoia.Cef.Internal;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefLoadHandler {
        public CefBase Base;
        public IntPtr OnLoadingStateChange;
        public IntPtr OnLoadStart;
        public IntPtr OnLoadEnd;
        public IntPtr OnLoadError;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefLoadHandlerCapiDelegates {
        public delegate void OnLoadingStateChangeCallback(
            IntPtr self, IntPtr browser, int isloading, int cangoback, int cangoforward);

        public delegate void OnLoadStartCallback(IntPtr self, IntPtr browser, IntPtr frame);

        public delegate void OnLoadEndCallback(IntPtr self, IntPtr browser, IntPtr frame, int httpstatuscode);

        public delegate void OnLoadErrorCallback(
            IntPtr self, IntPtr browser, IntPtr frame, CefErrorcode errorcode, IntPtr errortext, IntPtr failedurl);
    }
}
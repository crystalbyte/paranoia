#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefLifeSpanHandler {
        public CefBase Base;
        public IntPtr OnBeforePopup;
        public IntPtr OnAfterCreated;
        public IntPtr RunModal;
        public IntPtr DoClose;
        public IntPtr OnBeforeClose;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefLifeSpanHandlerCapiDelegates {
        public delegate int OnBeforePopupCallback(
            IntPtr self, IntPtr browser, IntPtr frame, IntPtr targetUrl, IntPtr targetFrameName, IntPtr popupfeatures,
            IntPtr windowinfo, IntPtr client, IntPtr settings, ref int noJavascriptAccess);

        public delegate void OnAfterCreatedCallback(IntPtr self, IntPtr browser);

        public delegate int RunModalCallback(IntPtr self, IntPtr browser);

        public delegate int DoCloseCallback(IntPtr self, IntPtr browser);

        public delegate void OnBeforeCloseCallback(IntPtr self, IntPtr browser);
    }
}
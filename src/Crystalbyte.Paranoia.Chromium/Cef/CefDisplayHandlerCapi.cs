#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefDisplayHandler {
        public CefBase Base;
        public IntPtr OnAddressChange;
        public IntPtr OnTitleChange;
        public IntPtr OnFaviconUrlchange;
        public IntPtr OnTooltip;
        public IntPtr OnStatusMessage;
        public IntPtr OnConsoleMessage;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefDisplayHandlerCapiDelegates {
        public delegate void OnAddressChangeCallback(IntPtr self, IntPtr browser, IntPtr frame, IntPtr url);

        public delegate void OnTitleChangeCallback(IntPtr self, IntPtr browser, IntPtr title);

        public delegate void OnFaviconUrlchangeCallback(IntPtr self, IntPtr browser, IntPtr iconUrls);

        public delegate int OnTooltipCallback(IntPtr self, IntPtr browser, IntPtr text);

        public delegate void OnStatusMessageCallback(IntPtr self, IntPtr browser, IntPtr value);

        public delegate int OnConsoleMessageCallback(
            IntPtr self, IntPtr browser, IntPtr message, IntPtr source, int line);
    }
}
#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefResourceBundleHandler {
        public CefBase Base;
        public IntPtr GetLocalizedString;
        public IntPtr GetDataResource;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefResourceBundleHandlerCapiDelegates {
        public delegate int GetLocalizedStringCallback(IntPtr self, int messageId, IntPtr @string);

        public delegate int GetDataResourceCallback(IntPtr self, int resourceId, IntPtr data, ref int dataSize);
    }
}
#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefFindHandler {
        public CefBase Base;
        public IntPtr OnFindResult;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefFindHandlerCapiDelegates {
        public delegate void OnFindResultCallback(
            IntPtr self, IntPtr browser, int identifier, int count, IntPtr selectionrect, int activematchordinal,
            int finalupdate);
    }
}
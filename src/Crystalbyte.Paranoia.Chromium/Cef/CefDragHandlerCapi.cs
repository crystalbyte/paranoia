#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;
using Crystalbyte.Paranoia.Cef.Internal;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefDragHandler {
        public CefBase Base;
        public IntPtr OnDragEnter;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefDragHandlerCapiDelegates {
        public delegate int OnDragEnterCallback(IntPtr self, IntPtr browser, IntPtr dragdata, CefDragOperationsMask mask
            );
    }
}
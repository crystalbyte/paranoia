#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefStringVisitor {
        public CefBase Base;
        public IntPtr Visit;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefStringVisitorCapiDelegates {
        public delegate void VisitCallback4(IntPtr self, IntPtr @string);
    }
}
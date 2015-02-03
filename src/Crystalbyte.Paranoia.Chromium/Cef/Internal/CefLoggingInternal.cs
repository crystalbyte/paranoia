#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef.Internal {
    [SuppressUnmanagedCodeSecurity]
    public static class CefLoggingInternalClass {
        [DllImport(CefAssembly.Name, EntryPoint = "cef_get_min_log_level", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern int CefGetMinLogLevel();

        [DllImport(CefAssembly.Name, EntryPoint = "cef_get_vlog_level", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern int CefGetVlogLevel(IntPtr fileStart, int n);

        [DllImport(CefAssembly.Name, EntryPoint = "cef_log", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern void CefLog(IntPtr file, int line, int severity, IntPtr message);
    }
}
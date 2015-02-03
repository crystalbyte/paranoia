#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [SuppressUnmanagedCodeSecurity]
    public static class CefTraceCapi {
        [DllImport(CefAssembly.Name, EntryPoint = "cef_begin_tracing", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern int CefBeginTracing(IntPtr categories, IntPtr callback);

        [DllImport(CefAssembly.Name, EntryPoint = "cef_end_tracing", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern int CefEndTracing(IntPtr tracingFile, IntPtr callback);

        [DllImport(CefAssembly.Name, EntryPoint = "cef_now_from_system_trace_time",
            CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern long CefNowFromSystemTraceTime();
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefEndTracingCallback {
        public CefBase Base;
        public IntPtr OnEndTracingComplete;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefTraceCapiDelegates {
        public delegate void OnEndTracingCompleteCallback(IntPtr self, IntPtr tracingFile);
    }
}
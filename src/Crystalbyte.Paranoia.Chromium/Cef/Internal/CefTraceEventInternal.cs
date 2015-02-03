#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef.Internal {
    [SuppressUnmanagedCodeSecurity]
    public static class CefTraceEventInternalClass {
        [DllImport(CefAssembly.Name, EntryPoint = "cef_trace_event_instant", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern void CefTraceEventInstant(IntPtr category, IntPtr name, IntPtr arg1Name, ulong arg1Val,
            IntPtr arg2Name, ulong arg2Val, int copy);

        [DllImport(CefAssembly.Name, EntryPoint = "cef_trace_event_begin", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern void CefTraceEventBegin(IntPtr category, IntPtr name, IntPtr arg1Name, ulong arg1Val,
            IntPtr arg2Name, ulong arg2Val, int copy);

        [DllImport(CefAssembly.Name, EntryPoint = "cef_trace_event_end", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern void CefTraceEventEnd(IntPtr category, IntPtr name, IntPtr arg1Name, ulong arg1Val,
            IntPtr arg2Name, ulong arg2Val, int copy);

        [DllImport(CefAssembly.Name, EntryPoint = "cef_trace_counter", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern void CefTraceCounter(IntPtr category, IntPtr name, IntPtr value1Name, ulong value1Val,
            IntPtr value2Name, ulong value2Val, int copy);

        [DllImport(CefAssembly.Name, EntryPoint = "cef_trace_counter_id", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern void CefTraceCounterId(IntPtr category, IntPtr name, ulong id, IntPtr value1Name,
            ulong value1Val, IntPtr value2Name, ulong value2Val, int copy);

        [DllImport(CefAssembly.Name, EntryPoint = "cef_trace_event_async_begin",
            CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void CefTraceEventAsyncBegin(IntPtr category, IntPtr name, ulong id, IntPtr arg1Name,
            ulong arg1Val, IntPtr arg2Name, ulong arg2Val, int copy);

        [DllImport(CefAssembly.Name, EntryPoint = "cef_trace_event_async_step_into",
            CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void CefTraceEventAsyncStepInto(IntPtr category, IntPtr name, ulong id, ulong step,
            IntPtr arg1Name, ulong arg1Val, int copy);

        [DllImport(CefAssembly.Name, EntryPoint = "cef_trace_event_async_step_past",
            CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void CefTraceEventAsyncStepPast(IntPtr category, IntPtr name, ulong id, ulong step,
            IntPtr arg1Name, ulong arg1Val, int copy);

        [DllImport(CefAssembly.Name, EntryPoint = "cef_trace_event_async_end",
            CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void CefTraceEventAsyncEnd(IntPtr category, IntPtr name, ulong id, IntPtr arg1Name,
            ulong arg1Val, IntPtr arg2Name, ulong arg2Val, int copy);
    }
}
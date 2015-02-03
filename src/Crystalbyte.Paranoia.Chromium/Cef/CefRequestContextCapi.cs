#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [SuppressUnmanagedCodeSecurity]
    public static class CefRequestContextCapi {
        [DllImport(CefAssembly.Name, EntryPoint = "cef_request_context_get_global_context",
            CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern IntPtr CefRequestContextGetGlobalContext();

        [DllImport(CefAssembly.Name, EntryPoint = "cef_request_context_create_context",
            CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern IntPtr CefRequestContextCreateContext(IntPtr handler);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefRequestContext {
        public CefBase Base;
        public IntPtr IsSame;
        public IntPtr IsGlobal;
        public IntPtr GetHandler;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefRequestContextCapiDelegates {
        public delegate int IsSameCallback3(IntPtr self, IntPtr other);

        public delegate int IsGlobalCallback(IntPtr self);

        public delegate IntPtr GetHandlerCallback(IntPtr self);
    }
}
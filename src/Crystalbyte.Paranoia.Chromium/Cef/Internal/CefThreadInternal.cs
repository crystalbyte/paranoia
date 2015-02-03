#region Using directives

using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef.Internal {
    [SuppressUnmanagedCodeSecurity]
    public static class CefThreadInternalClass {
        [DllImport(CefAssembly.Name, EntryPoint = "cef_get_current_platform_thread_id",
            CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern uint CefGetCurrentPlatformThreadId();

        [DllImport(CefAssembly.Name, EntryPoint = "cef_get_current_platform_thread_handle",
            CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern uint CefGetCurrentPlatformThreadHandle();
    }
}
#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;
using Crystalbyte.Paranoia.Cef.Internal;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [SuppressUnmanagedCodeSecurity]
    public static class CefPathUtilCapi {
        [DllImport(CefAssembly.Name, EntryPoint = "cef_get_path", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern int CefGetPath(CefPathKey key, IntPtr path);
    }
}
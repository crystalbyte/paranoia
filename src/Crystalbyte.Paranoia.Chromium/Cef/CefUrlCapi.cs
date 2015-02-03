#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [SuppressUnmanagedCodeSecurity]
    public static class CefUrlCapi {
        [DllImport(CefAssembly.Name, EntryPoint = "cef_parse_url", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern int CefParseUrl(IntPtr url, IntPtr parts);

        [DllImport(CefAssembly.Name, EntryPoint = "cef_create_url", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern int CefCreateUrl(IntPtr parts, IntPtr url);

        [DllImport(CefAssembly.Name, EntryPoint = "cef_get_mime_type", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern IntPtr CefGetMimeType(IntPtr extension);

        [DllImport(CefAssembly.Name, EntryPoint = "cef_get_extensions_for_mime_type",
            CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void CefGetExtensionsForMimeType(IntPtr mimeType, IntPtr extensions);
    }
}
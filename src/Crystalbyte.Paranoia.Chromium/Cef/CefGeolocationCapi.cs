#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [SuppressUnmanagedCodeSecurity]
    public static class CefGeolocationCapi {
        [DllImport(CefAssembly.Name, EntryPoint = "cef_get_geolocation", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern int CefGetGeolocation(IntPtr callback);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefGetGeolocationCallback {
        public CefBase Base;
        public IntPtr OnLocationUpdate;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefGeolocationCapiDelegates {
        public delegate void OnLocationUpdateCallback(IntPtr self, IntPtr position);
    }
}
#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefBrowserProcessHandler {
        public CefBase Base;
        public IntPtr OnContextInitialized;
        public IntPtr OnBeforeChildProcessLaunch;
        public IntPtr OnRenderProcessThreadCreated;
        public IntPtr GetPrintHandler;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefBrowserProcessHandlerCapiDelegates {
        public delegate void OnContextInitializedCallback(IntPtr self);

        public delegate void OnBeforeChildProcessLaunchCallback(IntPtr self, IntPtr commandLine);

        public delegate void OnRenderProcessThreadCreatedCallback(IntPtr self, IntPtr extraInfo);

        public delegate IntPtr GetPrintHandlerCallback(IntPtr self);
    }
}
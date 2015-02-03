#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;
using Crystalbyte.Paranoia.Cef.Internal;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefRenderProcessHandler {
        public CefBase Base;
        public IntPtr OnRenderThreadCreated;
        public IntPtr OnWebKitInitialized;
        public IntPtr OnBrowserCreated;
        public IntPtr OnBrowserDestroyed;
        public IntPtr GetLoadHandler;
        public IntPtr OnBeforeNavigation;
        public IntPtr OnContextCreated;
        public IntPtr OnContextReleased;
        public IntPtr OnUncaughtException;
        public IntPtr OnFocusedNodeChanged;
        public IntPtr OnProcessMessageReceived;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefRenderProcessHandlerCapiDelegates {
        public delegate void OnRenderThreadCreatedCallback(IntPtr self, IntPtr extraInfo);

        public delegate void OnWebKitInitializedCallback(IntPtr self);

        public delegate void OnBrowserCreatedCallback(IntPtr self, IntPtr browser);

        public delegate void OnBrowserDestroyedCallback(IntPtr self, IntPtr browser);

        public delegate IntPtr GetLoadHandlerCallback2(IntPtr self);

        public delegate int OnBeforeNavigationCallback(
            IntPtr self, IntPtr browser, IntPtr frame, IntPtr request, CefNavigationType navigationType, int isRedirect);

        public delegate void OnContextCreatedCallback(IntPtr self, IntPtr browser, IntPtr frame, IntPtr context);

        public delegate void OnContextReleasedCallback(IntPtr self, IntPtr browser, IntPtr frame, IntPtr context);

        public delegate void OnUncaughtExceptionCallback(
            IntPtr self, IntPtr browser, IntPtr frame, IntPtr context, IntPtr exception, IntPtr stacktrace);

        public delegate void OnFocusedNodeChangedCallback(IntPtr self, IntPtr browser, IntPtr frame, IntPtr node);

        public delegate int OnProcessMessageReceivedCallback2(
            IntPtr self, IntPtr browser, CefProcessId sourceProcess, IntPtr message);
    }
}
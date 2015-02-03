#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;
using Crystalbyte.Paranoia.Cef.Internal;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefClient {
        public CefBase Base;
        public IntPtr GetContextMenuHandler;
        public IntPtr GetDialogHandler;
        public IntPtr GetDisplayHandler;
        public IntPtr GetDownloadHandler;
        public IntPtr GetDragHandler;
        public IntPtr GetFindHandler;
        public IntPtr GetFocusHandler;
        public IntPtr GetGeolocationHandler;
        public IntPtr GetJsdialogHandler;
        public IntPtr GetKeyboardHandler;
        public IntPtr GetLifeSpanHandler;
        public IntPtr GetLoadHandler;
        public IntPtr GetRenderHandler;
        public IntPtr GetRequestHandler;
        public IntPtr OnProcessMessageReceived;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefClientCapiDelegates {
        public delegate IntPtr GetContextMenuHandlerCallback(IntPtr self);

        public delegate IntPtr GetDialogHandlerCallback(IntPtr self);

        public delegate IntPtr GetDisplayHandlerCallback(IntPtr self);

        public delegate IntPtr GetDownloadHandlerCallback(IntPtr self);

        public delegate IntPtr GetDragHandlerCallback(IntPtr self);

        public delegate IntPtr GetFindHandlerCallback(IntPtr self);

        public delegate IntPtr GetFocusHandlerCallback(IntPtr self);

        public delegate IntPtr GetGeolocationHandlerCallback(IntPtr self);

        public delegate IntPtr GetJsdialogHandlerCallback(IntPtr self);

        public delegate IntPtr GetKeyboardHandlerCallback(IntPtr self);

        public delegate IntPtr GetLifeSpanHandlerCallback(IntPtr self);

        public delegate IntPtr GetLoadHandlerCallback(IntPtr self);

        public delegate IntPtr GetRenderHandlerCallback(IntPtr self);

        public delegate IntPtr GetRequestHandlerCallback(IntPtr self);

        public delegate int OnProcessMessageReceivedCallback(
            IntPtr self, IntPtr browser, CefProcessId sourceProcess, IntPtr message);
    }
}
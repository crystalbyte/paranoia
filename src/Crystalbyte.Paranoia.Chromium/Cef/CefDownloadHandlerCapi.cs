#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefBeforeDownloadCallback {
        public CefBase Base;
        public IntPtr Cont;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefDownloadItemCallback {
        public CefBase Base;
        public IntPtr Cancel;
        public IntPtr Pause;
        public IntPtr Resume;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefDownloadHandler {
        public CefBase Base;
        public IntPtr OnBeforeDownload;
        public IntPtr OnDownloadUpdated;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefDownloadHandlerCapiDelegates {
        public delegate void ContCallback4(IntPtr self, IntPtr downloadPath, int showDialog);

        public delegate void CancelCallback4(IntPtr self);

        public delegate void PauseCallback(IntPtr self);

        public delegate void ResumeCallback(IntPtr self);

        public delegate void OnBeforeDownloadCallback(
            IntPtr self, IntPtr browser, IntPtr downloadItem, IntPtr suggestedName, IntPtr callback);

        public delegate void OnDownloadUpdatedCallback(IntPtr self, IntPtr browser, IntPtr downloadItem, IntPtr callback
            );
    }
}
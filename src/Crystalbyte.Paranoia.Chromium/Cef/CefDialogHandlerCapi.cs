#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;
using Crystalbyte.Paranoia.Cef.Internal;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefFileDialogCallback {
        public CefBase Base;
        public IntPtr Cont;
        public IntPtr Cancel;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefDialogHandler {
        public CefBase Base;
        public IntPtr OnFileDialog;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefDialogHandlerCapiDelegates {
        public delegate void ContCallback3(IntPtr self, int selectedAcceptFilter, IntPtr filePaths);

        public delegate void CancelCallback3(IntPtr self);

        public delegate int OnFileDialogCallback(
            IntPtr self, IntPtr browser, CefFileDialogMode mode, IntPtr title, IntPtr defaultFilePath,
            IntPtr acceptFilters, int selectedAcceptFilter, IntPtr callback);
    }
}
#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefPrintDialogCallback {
        public CefBase Base;
        public IntPtr Cont;
        public IntPtr Cancel;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefPrintJobCallback {
        public CefBase Base;
        public IntPtr Cont;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefPrintHandler {
        public CefBase Base;
        public IntPtr OnPrintSettings;
        public IntPtr OnPrintDialog;
        public IntPtr OnPrintJob;
        public IntPtr OnPrintReset;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefPrintHandlerCapiDelegates {
        public delegate void ContCallback7(IntPtr self, IntPtr settings);

        public delegate void CancelCallback5(IntPtr self);

        public delegate void ContCallback8(IntPtr self);

        public delegate void OnPrintSettingsCallback(IntPtr self, IntPtr settings, int getDefaults);

        public delegate int OnPrintDialogCallback(IntPtr self, int hasSelection, IntPtr callback);

        public delegate int OnPrintJobCallback(IntPtr self, IntPtr documentName, IntPtr pdfFilePath, IntPtr callback);

        public delegate void OnPrintResetCallback(IntPtr self);
    }
}
#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;
using Crystalbyte.Paranoia.Cef.Internal;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefJsdialogCallback {
        public CefBase Base;
        public IntPtr Cont;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefJsdialogHandler {
        public CefBase Base;
        public IntPtr OnJsdialog;
        public IntPtr OnBeforeUnloadDialog;
        public IntPtr OnResetDialogState;
        public IntPtr OnDialogClosed;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefJsdialogHandlerCapiDelegates {
        public delegate void ContCallback6(IntPtr self, int success, IntPtr userInput);

        public delegate int OnJsdialogCallback(
            IntPtr self, IntPtr browser, IntPtr originUrl, IntPtr acceptLang, CefJsdialogType dialogType,
            IntPtr messageText, IntPtr defaultPromptText, IntPtr callback, ref int suppressMessage);

        public delegate int OnBeforeUnloadDialogCallback(
            IntPtr self, IntPtr browser, IntPtr messageText, int isReload, IntPtr callback);

        public delegate void OnResetDialogStateCallback(IntPtr self, IntPtr browser);

        public delegate void OnDialogClosedCallback(IntPtr self, IntPtr browser);
    }
}
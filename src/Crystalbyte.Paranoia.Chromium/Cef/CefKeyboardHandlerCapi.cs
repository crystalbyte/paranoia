#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefKeyboardHandler {
        public CefBase Base;
        public IntPtr OnPreKeyEvent;
        public IntPtr OnKeyEvent;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefKeyboardHandlerCapiDelegates {
        public delegate int OnPreKeyEventCallback(
            IntPtr self, IntPtr browser, IntPtr @event, IntPtr osEvent, ref int isKeyboardShortcut);

        public delegate int OnKeyEventCallback(IntPtr self, IntPtr browser, IntPtr @event, IntPtr osEvent);
    }
}
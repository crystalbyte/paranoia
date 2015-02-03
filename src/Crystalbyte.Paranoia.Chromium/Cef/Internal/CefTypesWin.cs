#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef.Internal {
    [StructLayout(LayoutKind.Sequential)]
    public struct WindowsCefMainArgs {
        public IntPtr Instance;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WindowsCefWindowInfo {
        public uint ExStyle;
        public CefStringUtf16 WindowName;
        public uint Style;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public IntPtr ParentWindow;
        public IntPtr Menu;
        public int WindowlessRenderingEnabled;
        public int TransparentPaintingEnabled;
        public IntPtr Window;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefTypesWinDelegates {}
}
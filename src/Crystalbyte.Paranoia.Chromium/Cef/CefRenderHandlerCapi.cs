#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;
using Crystalbyte.Paranoia.Cef.Internal;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefRenderHandler {
        public CefBase Base;
        public IntPtr GetRootScreenRect;
        public IntPtr GetViewRect;
        public IntPtr GetScreenPoint;
        public IntPtr GetScreenInfo;
        public IntPtr OnPopupShow;
        public IntPtr OnPopupSize;
        public IntPtr OnPaint;
        public IntPtr OnCursorChange;
        public IntPtr StartDragging;
        public IntPtr UpdateDragCursor;
        public IntPtr OnScrollOffsetChanged;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefRenderHandlerCapiDelegates {
        public delegate int GetRootScreenRectCallback(IntPtr self, IntPtr browser, IntPtr rect);

        public delegate int GetViewRectCallback(IntPtr self, IntPtr browser, IntPtr rect);

        public delegate int GetScreenPointCallback(
            IntPtr self, IntPtr browser, int viewx, int viewy, ref int screenx, ref int screeny);

        public delegate int GetScreenInfoCallback(IntPtr self, IntPtr browser, IntPtr screenInfo);

        public delegate void OnPopupShowCallback(IntPtr self, IntPtr browser, int show);

        public delegate void OnPopupSizeCallback(IntPtr self, IntPtr browser, IntPtr rect);

        public delegate void OnPaintCallback(
            IntPtr self, IntPtr browser, CefPaintElementType type, int dirtyrectscount, CefRect dirtyrects,
            IntPtr buffer, int width, int height);

        public delegate void OnCursorChangeCallback(
            IntPtr self, IntPtr browser, IntPtr cursor, CefCursorType type, IntPtr customCursorInfo);

        public delegate int StartDraggingCallback(
            IntPtr self, IntPtr browser, IntPtr dragData, CefDragOperationsMask allowedOps, int x, int y);

        public delegate void UpdateDragCursorCallback(IntPtr self, IntPtr browser, CefDragOperationsMask operation);

        public delegate void OnScrollOffsetChangedCallback(IntPtr self, IntPtr browser);
    }
}
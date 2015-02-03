#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;
using Crystalbyte.Paranoia.Cef.Internal;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [SuppressUnmanagedCodeSecurity]
    public static class CefBrowserCapi {
        [DllImport(CefAssembly.Name, EntryPoint = "cef_browser_host_create_browser",
            CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int CefBrowserHostCreateBrowser(IntPtr windowinfo, IntPtr client, IntPtr url,
            IntPtr settings, IntPtr requestContext);

        [DllImport(CefAssembly.Name, EntryPoint = "cef_browser_host_create_browser_sync",
            CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern IntPtr CefBrowserHostCreateBrowserSync(IntPtr windowinfo, IntPtr client, IntPtr url,
            IntPtr settings, IntPtr requestContext);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefBrowser {
        public CefBase Base;
        public IntPtr GetHost;
        public IntPtr CanGoBack;
        public IntPtr GoBack;
        public IntPtr CanGoForward;
        public IntPtr GoForward;
        public IntPtr IsLoading;
        public IntPtr Reload;
        public IntPtr ReloadIgnoreCache;
        public IntPtr StopLoad;
        public IntPtr GetIdentifier;
        public IntPtr IsSame;
        public IntPtr IsPopup;
        public IntPtr HasDocument;
        public IntPtr GetMainFrame;
        public IntPtr GetFocusedFrame;
        public IntPtr GetFrameByident;
        public IntPtr GetFrame;
        public IntPtr GetFrameCount;
        public IntPtr GetFrameIdentifiers;
        public IntPtr GetFrameNames;
        public IntPtr SendProcessMessage;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefRunFileDialogCallback {
        public CefBase Base;
        public IntPtr OnFileDialogDismissed;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefNavigationEntryVisitor {
        public CefBase Base;
        public IntPtr Visit;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefBrowserHost {
        public CefBase Base;
        public IntPtr GetBrowser;
        public IntPtr CloseBrowser;
        public IntPtr SetFocus;
        public IntPtr SetWindowVisibility;
        public IntPtr GetWindowHandle;
        public IntPtr GetOpenerWindowHandle;
        public IntPtr GetClient;
        public IntPtr GetRequestContext;
        public IntPtr GetZoomLevel;
        public IntPtr SetZoomLevel;
        public IntPtr RunFileDialog;
        public IntPtr StartDownload;
        public IntPtr Print;
        public IntPtr Find;
        public IntPtr StopFinding;
        public IntPtr ShowDevTools;
        public IntPtr CloseDevTools;
        public IntPtr GetNavigationEntries;
        public IntPtr SetMouseCursorChangeDisabled;
        public IntPtr IsMouseCursorChangeDisabled;
        public IntPtr ReplaceMisspelling;
        public IntPtr AddWordToDictionary;
        public IntPtr IsWindowRenderingDisabled;
        public IntPtr WasResized;
        public IntPtr WasHidden;
        public IntPtr NotifyScreenInfoChanged;
        public IntPtr Invalidate;
        public IntPtr SendKeyEvent;
        public IntPtr SendMouseClickEvent;
        public IntPtr SendMouseMoveEvent;
        public IntPtr SendMouseWheelEvent;
        public IntPtr SendFocusEvent;
        public IntPtr SendCaptureLostEvent;
        public IntPtr NotifyMoveOrResizeStarted;
        public IntPtr GetNstextInputContext;
        public IntPtr HandleKeyEventBeforeTextInputClient;
        public IntPtr HandleKeyEventAfterTextInputClient;
        public IntPtr DragTargetDragEnter;
        public IntPtr DragTargetDragOver;
        public IntPtr DragTargetDragLeave;
        public IntPtr DragTargetDrop;
        public IntPtr DragSourceEndedAt;
        public IntPtr DragSourceSystemDragEnded;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefBrowserCapiDelegates {
        public delegate IntPtr GetHostCallback(IntPtr self);

        public delegate int CanGoBackCallback(IntPtr self);

        public delegate void GoBackCallback(IntPtr self);

        public delegate int CanGoForwardCallback(IntPtr self);

        public delegate void GoForwardCallback(IntPtr self);

        public delegate int IsLoadingCallback(IntPtr self);

        public delegate void ReloadCallback(IntPtr self);

        public delegate void ReloadIgnoreCacheCallback(IntPtr self);

        public delegate void StopLoadCallback(IntPtr self);

        public delegate int GetIdentifierCallback(IntPtr self);

        public delegate int IsSameCallback(IntPtr self, IntPtr that);

        public delegate int IsPopupCallback(IntPtr self);

        public delegate int HasDocumentCallback(IntPtr self);

        public delegate IntPtr GetMainFrameCallback(IntPtr self);

        public delegate IntPtr GetFocusedFrameCallback(IntPtr self);

        public delegate IntPtr GetFrameByidentCallback(IntPtr self, long identifier);

        public delegate IntPtr GetFrameCallback(IntPtr self, IntPtr name);

        public delegate int GetFrameCountCallback(IntPtr self);

        public delegate void GetFrameIdentifiersCallback(IntPtr self, ref int identifierscount, ref long identifiers);

        public delegate void GetFrameNamesCallback(IntPtr self, IntPtr names);

        public delegate int SendProcessMessageCallback(IntPtr self, CefProcessId targetProcess, IntPtr message);

        public delegate void OnFileDialogDismissedCallback(IntPtr self, int selectedAcceptFilter, IntPtr filePaths);

        public delegate int VisitCallback(IntPtr self, IntPtr entry, int current, int index, int total);

        public delegate IntPtr GetBrowserCallback(IntPtr self);

        public delegate void CloseBrowserCallback(IntPtr self, int forceClose);

        public delegate void SetFocusCallback(IntPtr self, int focus);

        public delegate void SetWindowVisibilityCallback(IntPtr self, int visible);

        public delegate IntPtr GetWindowHandleCallback(IntPtr self);

        public delegate IntPtr GetOpenerWindowHandleCallback(IntPtr self);

        public delegate IntPtr GetClientCallback(IntPtr self);

        public delegate IntPtr GetRequestContextCallback(IntPtr self);

        public delegate Double GetZoomLevelCallback(IntPtr self);

        public delegate void SetZoomLevelCallback(IntPtr self, Double zoomlevel);

        public delegate void RunFileDialogCallback(
            IntPtr self, CefFileDialogMode mode, IntPtr title, IntPtr defaultFilePath, IntPtr acceptFilters,
            int selectedAcceptFilter, IntPtr callback);

        public delegate void StartDownloadCallback(IntPtr self, IntPtr url);

        public delegate void PrintCallback(IntPtr self);

        public delegate void FindCallback(
            IntPtr self, int identifier, IntPtr searchtext, int forward, int matchcase, int findnext);

        public delegate void StopFindingCallback(IntPtr self, int clearselection);

        public delegate void ShowDevToolsCallback(
            IntPtr self, IntPtr windowinfo, IntPtr client, IntPtr settings, IntPtr inspectElementAt);

        public delegate void CloseDevToolsCallback(IntPtr self);

        public delegate void GetNavigationEntriesCallback(IntPtr self, IntPtr visitor, int currentOnly);

        public delegate void SetMouseCursorChangeDisabledCallback(IntPtr self, int disabled);

        public delegate int IsMouseCursorChangeDisabledCallback(IntPtr self);

        public delegate void ReplaceMisspellingCallback(IntPtr self, IntPtr word);

        public delegate void AddWordToDictionaryCallback(IntPtr self, IntPtr word);

        public delegate int IsWindowRenderingDisabledCallback(IntPtr self);

        public delegate void WasResizedCallback(IntPtr self);

        public delegate void WasHiddenCallback(IntPtr self, int hidden);

        public delegate void NotifyScreenInfoChangedCallback(IntPtr self);

        public delegate void InvalidateCallback(IntPtr self, CefPaintElementType type);

        public delegate void SendKeyEventCallback(IntPtr self, IntPtr @event);

        public delegate void SendMouseClickEventCallback(
            IntPtr self, IntPtr @event, CefMouseButtonType type, int mouseup, int clickcount);

        public delegate void SendMouseMoveEventCallback(IntPtr self, IntPtr @event, int mouseleave);

        public delegate void SendMouseWheelEventCallback(IntPtr self, IntPtr @event, int deltax, int deltay);

        public delegate void SendFocusEventCallback(IntPtr self, int setfocus);

        public delegate void SendCaptureLostEventCallback(IntPtr self);

        public delegate void NotifyMoveOrResizeStartedCallback(IntPtr self);

        public delegate IntPtr GetNstextInputContextCallback(IntPtr self);

        public delegate void HandleKeyEventBeforeTextInputClientCallback(IntPtr self, IntPtr keyevent);

        public delegate void HandleKeyEventAfterTextInputClientCallback(IntPtr self, IntPtr keyevent);

        public delegate void DragTargetDragEnterCallback(
            IntPtr self, IntPtr dragData, IntPtr @event, CefDragOperationsMask allowedOps);

        public delegate void DragTargetDragOverCallback(IntPtr self, IntPtr @event, CefDragOperationsMask allowedOps);

        public delegate void DragTargetDragLeaveCallback(IntPtr self);

        public delegate void DragTargetDropCallback(IntPtr self, IntPtr @event);

        public delegate void DragSourceEndedAtCallback(IntPtr self, int x, int y, CefDragOperationsMask op);

        public delegate void DragSourceSystemDragEndedCallback(IntPtr self);
    }
}
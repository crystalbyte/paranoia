#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;
using Crystalbyte.Paranoia.Cef.Internal;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefNavigationEntry {
        public CefBase Base;
        public IntPtr IsValid;
        public IntPtr GetUrl;
        public IntPtr GetDisplayUrl;
        public IntPtr GetOriginalUrl;
        public IntPtr GetTitle;
        public IntPtr GetTransitionType;
        public IntPtr HasPostData;
        public IntPtr GetFrameName;
        public IntPtr GetCompletionTime;
        public IntPtr GetHttpStatusCode;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefNavigationEntryCapiDelegates {
        public delegate int IsValidCallback4(IntPtr self);

        public delegate IntPtr GetUrlCallback3(IntPtr self);

        public delegate IntPtr GetDisplayUrlCallback(IntPtr self);

        public delegate IntPtr GetOriginalUrlCallback2(IntPtr self);

        public delegate IntPtr GetTitleCallback2(IntPtr self);

        public delegate CefTransitionType GetTransitionTypeCallback(IntPtr self);

        public delegate int HasPostDataCallback(IntPtr self);

        public delegate IntPtr GetFrameNameCallback(IntPtr self);

        public delegate IntPtr GetCompletionTimeCallback(IntPtr self);

        public delegate int GetHttpStatusCodeCallback(IntPtr self);
    }
}
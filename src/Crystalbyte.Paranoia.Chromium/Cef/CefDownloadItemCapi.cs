#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefDownloadItem {
        public CefBase Base;
        public IntPtr IsValid;
        public IntPtr IsInProgress;
        public IntPtr IsComplete;
        public IntPtr IsCanceled;
        public IntPtr GetCurrentSpeed;
        public IntPtr GetPercentComplete;
        public IntPtr GetTotalBytes;
        public IntPtr GetReceivedBytes;
        public IntPtr GetStartTime;
        public IntPtr GetEndTime;
        public IntPtr GetFullPath;
        public IntPtr GetId;
        public IntPtr GetUrl;
        public IntPtr GetOriginalUrl;
        public IntPtr GetSuggestedFileName;
        public IntPtr GetContentDisposition;
        public IntPtr GetMimeType;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefDownloadItemCapiDelegates {
        public delegate int IsValidCallback2(IntPtr self);

        public delegate int IsInProgressCallback(IntPtr self);

        public delegate int IsCompleteCallback(IntPtr self);

        public delegate int IsCanceledCallback(IntPtr self);

        public delegate long GetCurrentSpeedCallback(IntPtr self);

        public delegate int GetPercentCompleteCallback(IntPtr self);

        public delegate long GetTotalBytesCallback(IntPtr self);

        public delegate long GetReceivedBytesCallback(IntPtr self);

        public delegate IntPtr GetStartTimeCallback(IntPtr self);

        public delegate IntPtr GetEndTimeCallback(IntPtr self);

        public delegate IntPtr GetFullPathCallback(IntPtr self);

        public delegate uint GetIdCallback(IntPtr self);

        public delegate IntPtr GetUrlCallback(IntPtr self);

        public delegate IntPtr GetOriginalUrlCallback(IntPtr self);

        public delegate IntPtr GetSuggestedFileNameCallback(IntPtr self);

        public delegate IntPtr GetContentDispositionCallback(IntPtr self);

        public delegate IntPtr GetMimeTypeCallback(IntPtr self);
    }
}
#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;
using Crystalbyte.Paranoia.Cef.Internal;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [SuppressUnmanagedCodeSecurity]
    public static class CefUrlrequestCapi {
        [DllImport(CefAssembly.Name, EntryPoint = "cef_urlrequest_create", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern IntPtr CefUrlrequestCreate(IntPtr request, IntPtr client);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefUrlrequest {
        public CefBase Base;
        public IntPtr GetRequest;
        public IntPtr GetClient;
        public IntPtr GetRequestStatus;
        public IntPtr GetRequestError;
        public IntPtr GetResponse;
        public IntPtr Cancel;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefUrlrequestClient {
        public CefBase Base;
        public IntPtr OnRequestComplete;
        public IntPtr OnUploadProgress;
        public IntPtr OnDownloadProgress;
        public IntPtr OnDownloadData;
        public IntPtr GetAuthCredentials;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefUrlrequestCapiDelegates {
        public delegate IntPtr GetRequestCallback(IntPtr self);

        public delegate IntPtr GetClientCallback2(IntPtr self);

        public delegate CefUrlrequestStatus GetRequestStatusCallback(IntPtr self);

        public delegate CefErrorcode GetRequestErrorCallback(IntPtr self);

        public delegate IntPtr GetResponseCallback(IntPtr self);

        public delegate void CancelCallback8(IntPtr self);

        public delegate void OnRequestCompleteCallback(IntPtr self, IntPtr request);

        public delegate void OnUploadProgressCallback(IntPtr self, IntPtr request, long current, long total);

        public delegate void OnDownloadProgressCallback(IntPtr self, IntPtr request, long current, long total);

        public delegate void OnDownloadDataCallback(IntPtr self, IntPtr request, IntPtr data, int dataLength);

        public delegate int GetAuthCredentialsCallback2(
            IntPtr self, int isproxy, IntPtr host, int port, IntPtr realm, IntPtr scheme, IntPtr callback);
    }
}
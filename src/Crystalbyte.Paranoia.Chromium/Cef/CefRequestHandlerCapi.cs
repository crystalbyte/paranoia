#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;
using Crystalbyte.Paranoia.Cef.Internal;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefQuotaCallback {
        public CefBase Base;
        public IntPtr Cont;
        public IntPtr Cancel;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefAllowCertificateErrorCallback {
        public CefBase Base;
        public IntPtr Cont;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefRequestHandler {
        public CefBase Base;
        public IntPtr OnBeforeBrowse;
        public IntPtr OnBeforeResourceLoad;
        public IntPtr GetResourceHandler;
        public IntPtr OnResourceRedirect;
        public IntPtr GetAuthCredentials;
        public IntPtr OnQuotaRequest;
        public IntPtr OnProtocolExecution;
        public IntPtr OnCertificateError;
        public IntPtr OnBeforePluginLoad;
        public IntPtr OnPluginCrashed;
        public IntPtr OnRenderProcessTerminated;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefRequestHandlerCapiDelegates {
        public delegate void ContCallback9(IntPtr self, int allow);

        public delegate void CancelCallback6(IntPtr self);

        public delegate void ContCallback10(IntPtr self, int allow);

        public delegate int OnBeforeBrowseCallback(
            IntPtr self, IntPtr browser, IntPtr frame, IntPtr request, int isRedirect);

        public delegate int OnBeforeResourceLoadCallback(IntPtr self, IntPtr browser, IntPtr frame, IntPtr request);

        public delegate IntPtr GetResourceHandlerCallback(IntPtr self, IntPtr browser, IntPtr frame, IntPtr request);

        public delegate void OnResourceRedirectCallback(
            IntPtr self, IntPtr browser, IntPtr frame, IntPtr oldUrl, IntPtr newUrl);

        public delegate int GetAuthCredentialsCallback(
            IntPtr self, IntPtr browser, IntPtr frame, int isproxy, IntPtr host, int port, IntPtr realm, IntPtr scheme,
            IntPtr callback);

        public delegate int OnQuotaRequestCallback(
            IntPtr self, IntPtr browser, IntPtr originUrl, long newSize, IntPtr callback);

        public delegate void OnProtocolExecutionCallback(
            IntPtr self, IntPtr browser, IntPtr url, ref int allowOsExecution);

        public delegate int OnCertificateErrorCallback(
            IntPtr self, CefErrorcode certError, IntPtr requestUrl, IntPtr callback);

        public delegate int OnBeforePluginLoadCallback(
            IntPtr self, IntPtr browser, IntPtr url, IntPtr policyUrl, IntPtr info);

        public delegate void OnPluginCrashedCallback(IntPtr self, IntPtr browser, IntPtr pluginPath);

        public delegate void OnRenderProcessTerminatedCallback(IntPtr self, IntPtr browser, CefTerminationStatus status);
    }
}
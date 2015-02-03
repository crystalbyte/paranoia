#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [SuppressUnmanagedCodeSecurity]
    public static class CefDragDataCapi {
        [DllImport(CefAssembly.Name, EntryPoint = "cef_drag_data_create", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern IntPtr CefDragDataCreate();
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefDragData {
        public CefBase Base;
        public IntPtr Clone;
        public IntPtr IsReadOnly;
        public IntPtr IsLink;
        public IntPtr IsFragment;
        public IntPtr IsFile;
        public IntPtr GetLinkUrl;
        public IntPtr GetLinkTitle;
        public IntPtr GetLinkMetadata;
        public IntPtr GetFragmentText;
        public IntPtr GetFragmentHtml;
        public IntPtr GetFragmentBaseUrl;
        public IntPtr GetFileName;
        public IntPtr GetFileContents;
        public IntPtr GetFileNames;
        public IntPtr SetLinkUrl;
        public IntPtr SetLinkTitle;
        public IntPtr SetLinkMetadata;
        public IntPtr SetFragmentText;
        public IntPtr SetFragmentHtml;
        public IntPtr SetFragmentBaseUrl;
        public IntPtr ResetFileContents;
        public IntPtr AddFile;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefDragDataCapiDelegates {
        public delegate IntPtr CloneCallback(IntPtr self);

        public delegate int IsReadOnlyCallback2(IntPtr self);

        public delegate int IsLinkCallback(IntPtr self);

        public delegate int IsFragmentCallback(IntPtr self);

        public delegate int IsFileCallback(IntPtr self);

        public delegate IntPtr GetLinkUrlCallback2(IntPtr self);

        public delegate IntPtr GetLinkTitleCallback(IntPtr self);

        public delegate IntPtr GetLinkMetadataCallback(IntPtr self);

        public delegate IntPtr GetFragmentTextCallback(IntPtr self);

        public delegate IntPtr GetFragmentHtmlCallback(IntPtr self);

        public delegate IntPtr GetFragmentBaseUrlCallback(IntPtr self);

        public delegate IntPtr GetFileNameCallback(IntPtr self);

        public delegate int GetFileContentsCallback(IntPtr self, IntPtr writer);

        public delegate int GetFileNamesCallback(IntPtr self, IntPtr names);

        public delegate void SetLinkUrlCallback(IntPtr self, IntPtr url);

        public delegate void SetLinkTitleCallback(IntPtr self, IntPtr title);

        public delegate void SetLinkMetadataCallback(IntPtr self, IntPtr data);

        public delegate void SetFragmentTextCallback(IntPtr self, IntPtr text);

        public delegate void SetFragmentHtmlCallback(IntPtr self, IntPtr html);

        public delegate void SetFragmentBaseUrlCallback(IntPtr self, IntPtr baseUrl);

        public delegate void ResetFileContentsCallback(IntPtr self);

        public delegate void AddFileCallback(IntPtr self, IntPtr path, IntPtr displayName);
    }
}
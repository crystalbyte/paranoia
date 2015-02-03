#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;
using Crystalbyte.Paranoia.Cef.Internal;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefContextMenuHandler {
        public CefBase Base;
        public IntPtr OnBeforeContextMenu;
        public IntPtr OnContextMenuCommand;
        public IntPtr OnContextMenuDismissed;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefContextMenuParams {
        public CefBase Base;
        public IntPtr GetXcoord;
        public IntPtr GetYcoord;
        public IntPtr GetTypeFlags;
        public IntPtr GetLinkUrl;
        public IntPtr GetUnfilteredLinkUrl;
        public IntPtr GetSourceUrl;
        public IntPtr HasImageContents;
        public IntPtr GetPageUrl;
        public IntPtr GetFrameUrl;
        public IntPtr GetFrameCharset;
        public IntPtr GetMediaType;
        public IntPtr GetMediaStateFlags;
        public IntPtr GetSelectionText;
        public IntPtr GetMisspelledWord;
        public IntPtr GetMisspellingHash;
        public IntPtr GetDictionarySuggestions;
        public IntPtr IsEditable;
        public IntPtr IsSpellCheckEnabled;
        public IntPtr GetEditStateFlags;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefContextMenuHandlerCapiDelegates {
        public delegate void OnBeforeContextMenuCallback(
            IntPtr self, IntPtr browser, IntPtr frame, IntPtr @params, IntPtr model);

        public delegate int OnContextMenuCommandCallback(
            IntPtr self, IntPtr browser, IntPtr frame, IntPtr @params, int commandId, CefEventFlags eventFlags);

        public delegate void OnContextMenuDismissedCallback(IntPtr self, IntPtr browser, IntPtr frame);

        public delegate int GetXcoordCallback(IntPtr self);

        public delegate int GetYcoordCallback(IntPtr self);

        public delegate CefContextMenuTypeFlags GetTypeFlagsCallback(IntPtr self);

        public delegate IntPtr GetLinkUrlCallback(IntPtr self);

        public delegate IntPtr GetUnfilteredLinkUrlCallback(IntPtr self);

        public delegate IntPtr GetSourceUrlCallback(IntPtr self);

        public delegate int HasImageContentsCallback(IntPtr self);

        public delegate IntPtr GetPageUrlCallback(IntPtr self);

        public delegate IntPtr GetFrameUrlCallback(IntPtr self);

        public delegate IntPtr GetFrameCharsetCallback(IntPtr self);

        public delegate CefContextMenuMediaType GetMediaTypeCallback(IntPtr self);

        public delegate CefContextMenuMediaStateFlags GetMediaStateFlagsCallback(IntPtr self);

        public delegate IntPtr GetSelectionTextCallback(IntPtr self);

        public delegate IntPtr GetMisspelledWordCallback(IntPtr self);

        public delegate int GetMisspellingHashCallback(IntPtr self);

        public delegate int GetDictionarySuggestionsCallback(IntPtr self, IntPtr suggestions);

        public delegate int IsEditableCallback(IntPtr self);

        public delegate int IsSpellCheckEnabledCallback(IntPtr self);

        public delegate CefContextMenuEditStateFlags GetEditStateFlagsCallback(IntPtr self);
    }
}
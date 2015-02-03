#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;
using Crystalbyte.Paranoia.Cef.Internal;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [SuppressUnmanagedCodeSecurity]
    public static class CefPrintSettingsCapi {
        [DllImport(CefAssembly.Name, EntryPoint = "cef_print_settings_create",
            CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern IntPtr CefPrintSettingsCreate();
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefPrintSettings {
        public CefBase Base;
        public IntPtr IsValid;
        public IntPtr IsReadOnly;
        public IntPtr Copy;
        public IntPtr SetOrientation;
        public IntPtr IsLandscape;
        public IntPtr SetPrinterPrintableArea;
        public IntPtr SetDeviceName;
        public IntPtr GetDeviceName;
        public IntPtr SetDpi;
        public IntPtr GetDpi;
        public IntPtr SetPageRanges;
        public IntPtr GetPageRangesCount;
        public IntPtr GetPageRanges;
        public IntPtr SetSelectionOnly;
        public IntPtr IsSelectionOnly;
        public IntPtr SetCollate;
        public IntPtr WillCollate;
        public IntPtr SetColorModel;
        public IntPtr GetColorModel;
        public IntPtr SetCopies;
        public IntPtr GetCopies;
        public IntPtr SetDuplexMode;
        public IntPtr GetDuplexMode;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefPrintSettingsCapiDelegates {
        public delegate int IsValidCallback5(IntPtr self);

        public delegate int IsReadOnlyCallback3(IntPtr self);

        public delegate IntPtr CopyCallback3(IntPtr self);

        public delegate void SetOrientationCallback(IntPtr self, int landscape);

        public delegate int IsLandscapeCallback(IntPtr self);

        public delegate void SetPrinterPrintableAreaCallback(
            IntPtr self, IntPtr physicalSizeDeviceUnits, IntPtr printableAreaDeviceUnits, int landscapeNeedsFlip);

        public delegate void SetDeviceNameCallback(IntPtr self, IntPtr name);

        public delegate IntPtr GetDeviceNameCallback(IntPtr self);

        public delegate void SetDpiCallback(IntPtr self, int dpi);

        public delegate int GetDpiCallback(IntPtr self);

        public delegate void SetPageRangesCallback(IntPtr self, int rangescount, CefPageRange ranges);

        public delegate int GetPageRangesCountCallback(IntPtr self);

        public delegate void GetPageRangesCallback(IntPtr self, ref int rangescount, IntPtr ranges);

        public delegate void SetSelectionOnlyCallback(IntPtr self, int selectionOnly);

        public delegate int IsSelectionOnlyCallback(IntPtr self);

        public delegate void SetCollateCallback(IntPtr self, int collate);

        public delegate int WillCollateCallback(IntPtr self);

        public delegate void SetColorModelCallback(IntPtr self, CefColorModel model);

        public delegate CefColorModel GetColorModelCallback(IntPtr self);

        public delegate void SetCopiesCallback(IntPtr self, int copies);

        public delegate int GetCopiesCallback(IntPtr self);

        public delegate void SetDuplexModeCallback(IntPtr self, CefDuplexMode mode);

        public delegate CefDuplexMode GetDuplexModeCallback(IntPtr self);
    }
}
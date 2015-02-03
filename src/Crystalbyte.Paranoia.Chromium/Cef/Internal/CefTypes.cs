#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef.Internal {
    [StructLayout(LayoutKind.Sequential)]
    public struct CefSettings {
        public int Size;
        public int SingleProcess;
        public int NoSandbox;
        public CefStringUtf16 BrowserSubprocessPath;
        public int MultiThreadedMessageLoop;
        public int WindowlessRenderingEnabled;
        public int CommandLineArgsDisabled;
        public CefStringUtf16 CachePath;
        public int PersistSessionCookies;
        public CefStringUtf16 UserAgent;
        public CefStringUtf16 ProductVersion;
        public CefStringUtf16 Locale;
        public CefStringUtf16 LogFile;
        public CefLogSeverity LogSeverity;
        public CefStringUtf16 JavascriptFlags;
        public CefStringUtf16 ResourcesDirPath;
        public CefStringUtf16 LocalesDirPath;
        public int PackLoadingDisabled;
        public int RemoteDebuggingPort;
        public int UncaughtExceptionStackSize;
        public int ContextSafetyImplementation;
        public int IgnoreCertificateErrors;
        public uint BackgroundColor;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefBrowserSettings {
        public int Size;
        public int WindowlessFrameRate;
        public CefStringUtf16 StandardFontFamily;
        public CefStringUtf16 FixedFontFamily;
        public CefStringUtf16 SerifFontFamily;
        public CefStringUtf16 SansSerifFontFamily;
        public CefStringUtf16 CursiveFontFamily;
        public CefStringUtf16 FantasyFontFamily;
        public int DefaultFontSize;
        public int DefaultFixedFontSize;
        public int MinimumFontSize;
        public int MinimumLogicalFontSize;
        public CefStringUtf16 DefaultEncoding;
        public CefState RemoteFonts;
        public CefState Javascript;
        public CefState JavascriptOpenWindows;
        public CefState JavascriptCloseWindows;
        public CefState JavascriptAccessClipboard;
        public CefState JavascriptDomPaste;
        public CefState CaretBrowsing;
        public CefState Java;
        public CefState Plugins;
        public CefState UniversalAccessFromFileUrls;
        public CefState FileAccessFromFileUrls;
        public CefState WebSecurity;
        public CefState ImageLoading;
        public CefState ImageShrinkStandaloneToFit;
        public CefState TextAreaResize;
        public CefState TabToLinks;
        public CefState LocalStorage;
        public CefState Databases;
        public CefState ApplicationCache;
        public CefState Webgl;
        public uint BackgroundColor;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefUrlparts {
        public CefStringUtf16 Spec;
        public CefStringUtf16 Scheme;
        public CefStringUtf16 Username;
        public CefStringUtf16 Password;
        public CefStringUtf16 Host;
        public CefStringUtf16 Port;
        public CefStringUtf16 Origin;
        public CefStringUtf16 Path;
        public CefStringUtf16 Query;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefCookie {
        public CefStringUtf16 Name;
        public CefStringUtf16 Value;
        public CefStringUtf16 Domain;
        public CefStringUtf16 Path;
        public int Secure;
        public int Httponly;
        public IntPtr Creation;
        public IntPtr LastAccess;
        public int HasExpires;
        public IntPtr Expires;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefPoint {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefRect {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefSize {
        public int Width;
        public int Height;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefScreenInfo {
        public double DeviceScaleFactor;
        public int Depth;
        public int DepthPerComponent;
        public int IsMonochrome;
        public CefRect Rect;
        public CefRect AvailableRect;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefMouseEvent {
        public int X;
        public int Y;
        public uint Modifiers;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefKeyEvent {
        public CefKeyEventType Type;
        public uint Modifiers;
        public int WindowsKeyCode;
        public int NativeKeyCode;
        public int IsSystemKey;
        public char Character;
        public char UnmodifiedCharacter;
        public int FocusOnEditableField;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefPopupFeatures {
        public int X;
        public int Xset;
        public int Y;
        public int Yset;
        public int Width;
        public int Widthset;
        public int Height;
        public int Heightset;
        public int Menubarvisible;
        public int Statusbarvisible;
        public int Toolbarvisible;
        public int Locationbarvisible;
        public int Scrollbarsvisible;
        public int Resizable;
        public int Fullscreen;
        public int Dialog;
        public IntPtr Additionalfeatures;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefGeoposition {
        public Double Latitude;
        public Double Longitude;
        public Double Altitude;
        public Double Accuracy;
        public Double AltitudeAccuracy;
        public Double Heading;
        public Double Speed;
        public IntPtr Timestamp;
        public CefGeopositionErrorCode ErrorCode;
        public CefStringUtf16 ErrorMessage;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefPageRange {
        public int From;
        public int To;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefCursorInfo {
        public CefPoint Hotspot;
        public double ImageScaleFactor;
        public IntPtr Buffer;
        public CefSize Size;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefTypesDelegates {}

    public enum CefLogSeverity {
        LogseverityDefault,
        LogseverityVerbose,
        LogseverityInfo,
        LogseverityWarning,
        LogseverityError,
        LogseverityDisable = 99,
    }

    public enum CefState {
        StateDefault = 0,
        StateEnabled,
        StateDisabled,
    }

    public enum CefTerminationStatus {
        TsAbnormalTermination,
        TsProcessWasKilled,
        TsProcessCrashed,
    }

    public enum CefPathKey {
        PkDirCurrent,
        PkDirExe,
        PkDirModule,
        PkDirTemp,
        PkFileExe,
        PkFileModule,
    }

    public enum CefStorageType {
        StLocalstorage = 0,
        StSessionstorage,
    }

    public enum CefErrorcode {
        ErrNone = 0,
        ErrFailed = -2,
        ErrAborted = -3,
        ErrInvalidArgument = -4,
        ErrInvalidHandle = -5,
        ErrFileNotFound = -6,
        ErrTimedOut = -7,
        ErrFileTooBig = -8,
        ErrUnexpected = -9,
        ErrAccessDenied = -10,
        ErrNotImplemented = -11,
        ErrConnectionClosed = -100,
        ErrConnectionReset = -101,
        ErrConnectionRefused = -102,
        ErrConnectionAborted = -103,
        ErrConnectionFailed = -104,
        ErrNameNotResolved = -105,
        ErrInternetDisconnected = -106,
        ErrSslProtocolError = -107,
        ErrAddressInvalid = -108,
        ErrAddressUnreachable = -109,
        ErrSslClientAuthCertNeeded = -110,
        ErrTunnelConnectionFailed = -111,
        ErrNoSslVersionsEnabled = -112,
        ErrSslVersionOrCipherMismatch = -113,
        ErrSslRenegotiationRequested = -114,
        ErrCertCommonNameInvalid = -200,
        ErrCertDateInvalid = -201,
        ErrCertAuthorityInvalid = -202,
        ErrCertContainsErrors = -203,
        ErrCertNoRevocationMechanism = -204,
        ErrCertUnableToCheckRevocation = -205,
        ErrCertRevoked = -206,
        ErrCertInvalid = -207,
        ErrCertEnd = -208,
        ErrInvalidUrl = -300,
        ErrDisallowedUrlScheme = -301,
        ErrUnknownUrlScheme = -302,
        ErrTooManyRedirects = -310,
        ErrUnsafeRedirect = -311,
        ErrUnsafePort = -312,
        ErrInvalidResponse = -320,
        ErrInvalidChunkedEncoding = -321,
        ErrMethodNotSupported = -322,
        ErrUnexpectedProxyAuth = -323,
        ErrEmptyResponse = -324,
        ErrResponseHeadersTooBig = -325,
        ErrCacheMiss = -400,
        ErrInsecureResponse = -501,
    }

    public enum CefDragOperationsMask : uint {
        DragOperationNone = 0,
        DragOperationCopy = 1,
        DragOperationLink = 2,
        DragOperationGeneric = 4,
        DragOperationPrivate = 8,
        DragOperationMove = 16,
        DragOperationDelete = 32,
        DragOperationEvery = 0xFFFFFFFF,
    }

    public enum CefV8Accesscontrol {
        V8AccessControlDefault = 0,
        V8AccessControlAllCanRead = 1,
        V8AccessControlAllCanWrite = 1 << 1,
        V8AccessControlProhibitsOverwriting = 1 << 2,
    }

    public enum CefV8Propertyattribute {
        V8PropertyAttributeNone = 0, // Writeable, Enumerable,
        V8PropertyAttributeReadonly = 1 << 0, // Not writeable,
        V8PropertyAttributeDontenum = 1 << 1, // Not enumerable,
        V8PropertyAttributeDontdelete = 1 << 2 // Not configurable,
    }

    public enum CefPostdataelementType {
        PdeTypeEmpty = 0,
        PdeTypeBytes,
        PdeTypeFile,
    }

    public enum CefResourceType {
        RtMainFrame = 0,
        RtSubFrame,
        RtStylesheet,
        RtScript,
        RtImage,
        RtFontResource,
        RtSubResource,
        RtObject,
        RtMedia,
        RtWorker,
        RtSharedWorker,
        RtPrefetch,
        RtFavicon,
        RtXhr,
        RtPing,
        RtServiceWorker,
    }

    public enum CefTransitionType : uint {
        TtLink = 0,
        TtExplicit = 1,
        TtAutoSubframe = 3,
        TtManualSubframe = 4,
        TtFormSubmit = 7,
        TtReload = 8,
        TtSourceMask = 0xFF,
        TtBlockedFlag = 0x00800000,
        TtForwardBackFlag = 0x01000000,
        TtChainStartFlag = 0x10000000,
        TtChainEndFlag = 0x20000000,
        TtClientRedirectFlag = 0x40000000,
        TtServerRedirectFlag = 0x80000000,
        TtIsRedirectMask = 0xC0000000,
        TtQualifierMask = 0xFFFFFF00,
    }

    public enum CefUrlrequestFlags {
        UrFlagNone = 0,
        UrFlagSkipCache = 1 << 0,
        UrFlagAllowCachedCredentials = 1 << 1,
        UrFlagReportUploadProgress = 1 << 3,
        UrFlagReportRawHeaders = 1 << 5,
        UrFlagNoDownloadData = 1 << 6,
        UrFlagNoRetryOn5Xx = 1 << 7,
    }

    public enum CefUrlrequestStatus {
        UrUnknown = 0,
        UrSuccess,
        UrIoPending,
        UrCanceled,
        UrFailed,
    }

    public enum CefProcessId {
        PidBrowser,
        PidRenderer,
    }

    public enum CefThreadId {
        TidUi,
        TidDb,
        TidFile,
        TidFileUserBlocking,
        TidProcessLauncher,
        TidCache,
        TidIo,
        TidRenderer,
    }

    public enum CefValueType {
        VtypeInvalid = 0,
        VtypeNull,
        VtypeBool,
        VtypeInt,
        VtypeDouble,
        VtypeString,
        VtypeBinary,
        VtypeDictionary,
        VtypeList,
    }

    public enum CefJsdialogType {
        JsdialogtypeAlert = 0,
        JsdialogtypeConfirm,
        JsdialogtypePrompt,
    }

    public enum CefMenuId {
        MenuIdBack = 100,
        MenuIdForward = 101,
        MenuIdReload = 102,
        MenuIdReloadNocache = 103,
        MenuIdStopload = 104,
        MenuIdUndo = 110,
        MenuIdRedo = 111,
        MenuIdCut = 112,
        MenuIdCopy = 113,
        MenuIdPaste = 114,
        MenuIdDelete = 115,
        MenuIdSelectAll = 116,
        MenuIdFind = 130,
        MenuIdPrint = 131,
        MenuIdViewSource = 132,
        MenuIdSpellcheckSuggestion0 = 200,
        MenuIdSpellcheckSuggestion1 = 201,
        MenuIdSpellcheckSuggestion2 = 202,
        MenuIdSpellcheckSuggestion3 = 203,
        MenuIdSpellcheckSuggestion4 = 204,
        MenuIdSpellcheckSuggestionLast = 204,
        MenuIdNoSpellingSuggestions = 205,
        MenuIdAddToDictionary = 206,
        MenuIdUserFirst = 26500,
        MenuIdUserLast = 28500,
    }

    public enum CefMouseButtonType {
        MbtLeft = 0,
        MbtMiddle,
        MbtRight,
    }

    public enum CefPaintElementType {
        PetView = 0,
        PetPopup,
    }

    public enum CefEventFlags {
        EventflagNone = 0,
        EventflagCapsLockOn = 1 << 0,
        EventflagShiftDown = 1 << 1,
        EventflagControlDown = 1 << 2,
        EventflagAltDown = 1 << 3,
        EventflagLeftMouseButton = 1 << 4,
        EventflagMiddleMouseButton = 1 << 5,
        EventflagRightMouseButton = 1 << 6,
        EventflagCommandDown = 1 << 7,
        EventflagNumLockOn = 1 << 8,
        EventflagIsKeyPad = 1 << 9,
        EventflagIsLeft = 1 << 10,
        EventflagIsRight = 1 << 11,
    }

    public enum CefMenuItemType {
        MenuitemtypeNone,
        MenuitemtypeCommand,
        MenuitemtypeCheck,
        MenuitemtypeRadio,
        MenuitemtypeSeparator,
        MenuitemtypeSubmenu,
    }

    public enum CefContextMenuTypeFlags {
        CmTypeflagNone = 0,
        CmTypeflagPage = 1 << 0,
        CmTypeflagFrame = 1 << 1,
        CmTypeflagLink = 1 << 2,
        CmTypeflagMedia = 1 << 3,
        CmTypeflagSelection = 1 << 4,
        CmTypeflagEditable = 1 << 5,
    }

    public enum CefContextMenuMediaType {
        CmMediatypeNone,
        CmMediatypeImage,
        CmMediatypeVideo,
        CmMediatypeAudio,
        CmMediatypeFile,
        CmMediatypePlugin,
    }

    public enum CefContextMenuMediaStateFlags {
        CmMediaflagNone = 0,
        CmMediaflagError = 1 << 0,
        CmMediaflagPaused = 1 << 1,
        CmMediaflagMuted = 1 << 2,
        CmMediaflagLoop = 1 << 3,
        CmMediaflagCanSave = 1 << 4,
        CmMediaflagHasAudio = 1 << 5,
        CmMediaflagHasVideo = 1 << 6,
        CmMediaflagControlRootElement = 1 << 7,
        CmMediaflagCanPrint = 1 << 8,
        CmMediaflagCanRotate = 1 << 9,
    }

    public enum CefContextMenuEditStateFlags {
        CmEditflagNone = 0,
        CmEditflagCanUndo = 1 << 0,
        CmEditflagCanRedo = 1 << 1,
        CmEditflagCanCut = 1 << 2,
        CmEditflagCanCopy = 1 << 3,
        CmEditflagCanPaste = 1 << 4,
        CmEditflagCanDelete = 1 << 5,
        CmEditflagCanSelectAll = 1 << 6,
        CmEditflagCanTranslate = 1 << 7,
    }

    public enum CefKeyEventType {
        KeyeventRawkeydown = 0,
        KeyeventKeydown,
        KeyeventKeyup,
        KeyeventChar,
    }

    public enum CefFocusSource {
        FocusSourceNavigation = 0,
        FocusSourceSystem,
    }

    public enum CefNavigationType {
        NavigationLinkClicked = 0,
        NavigationFormSubmitted,
        NavigationBackForward,
        NavigationReload,
        NavigationFormResubmitted,
        NavigationOther,
    }

    public enum CefXmlEncodingType {
        XmlEncodingNone = 0,
        XmlEncodingUtf8,
        XmlEncodingUtf16le,
        XmlEncodingUtf16be,
        XmlEncodingAscii,
    }

    public enum CefXmlNodeType {
        XmlNodeUnsupported = 0,
        XmlNodeProcessingInstruction,
        XmlNodeDocumentType,
        XmlNodeElementStart,
        XmlNodeElementEnd,
        XmlNodeAttribute,
        XmlNodeText,
        XmlNodeCdata,
        XmlNodeEntityReference,
        XmlNodeWhitespace,
        XmlNodeComment,
    }

    public enum CefDomDocumentType {
        DomDocumentTypeUnknown = 0,
        DomDocumentTypeHtml,
        DomDocumentTypeXhtml,
        DomDocumentTypePlugin,
    }

    public enum CefDomEventCategory {
        DomEventCategoryUnknown = 0x0,
        DomEventCategoryUi = 0x1,
        DomEventCategoryMouse = 0x2,
        DomEventCategoryMutation = 0x4,
        DomEventCategoryKeyboard = 0x8,
        DomEventCategoryText = 0x10,
        DomEventCategoryComposition = 0x20,
        DomEventCategoryDrag = 0x40,
        DomEventCategoryClipboard = 0x80,
        DomEventCategoryMessage = 0x100,
        DomEventCategoryWheel = 0x200,
        DomEventCategoryBeforeTextInserted = 0x400,
        DomEventCategoryOverflow = 0x800,
        DomEventCategoryPageTransition = 0x1000,
        DomEventCategoryPopstate = 0x2000,
        DomEventCategoryProgress = 0x4000,
        DomEventCategoryXmlhttprequestProgress = 0x8000,
    }

    public enum CefDomEventPhase {
        DomEventPhaseUnknown = 0,
        DomEventPhaseCapturing,
        DomEventPhaseAtTarget,
        DomEventPhaseBubbling,
    }

    public enum CefDomNodeType {
        DomNodeTypeUnsupported = 0,
        DomNodeTypeElement,
        DomNodeTypeAttribute,
        DomNodeTypeText,
        DomNodeTypeCdataSection,
        DomNodeTypeProcessingInstructions,
        DomNodeTypeComment,
        DomNodeTypeDocument,
        DomNodeTypeDocumentType,
        DomNodeTypeDocumentFragment,
    }

    public enum CefFileDialogMode {
        FileDialogOpen = 0,
        FileDialogOpenMultiple,
        FileDialogOpenFolder,
        FileDialogSave,
        FileDialogTypeMask = 0xFF,
        FileDialogOverwritepromptFlag = 0x01000000,
        FileDialogHidereadonlyFlag = 0x02000000,
    }

    public enum CefGeopositionErrorCode {
        GeopositonErrorNone = 0,
        GeopositonErrorPermissionDenied,
        GeopositonErrorPositionUnavailable,
        GeopositonErrorTimeout,
    }

    public enum CefColorModel {
        ColorModelUnknown,
        ColorModelGray,
        ColorModelColor,
        ColorModelCmyk,
        ColorModelCmy,
        ColorModelKcmy,
        ColorModelCmyK, //CmyKRepresentsCmy+K.,
        ColorModelBlack,
        ColorModelGrayscale,
        ColorModelRgb,
        ColorModelRgb16,
        ColorModelRgba,
        ColorModelColormodeColor, //UsedInSamsungPrinterPpds.,
        ColorModelColormodeMonochrome, //UsedInSamsungPrinterPpds.,
        ColorModelHpColorColor, //UsedInHpColorPrinterPpds.,
        ColorModelHpColorBlack, //UsedInHpColorPrinterPpds.,
        ColorModelPrintoutmodeNormal, //UsedInFoomaticPpds.,
        ColorModelPrintoutmodeNormalGray, //UsedInFoomaticPpds.,
        ColorModelProcesscolormodelCmyk, //UsedInCanonPrinterPpds.,
        ColorModelProcesscolormodelGreyscale, //UsedInCanonPrinterPpds.,
        ColorModelProcesscolormodelRgb, //UsedInCanonPrinterPpds,
    }

    public enum CefDuplexMode {
        DuplexModeUnknown = -1,
        DuplexModeSimplex,
        DuplexModeLongEdge,
        DuplexModeShortEdge,
    }

    public enum CefCursorType {
        CtPointer = 0,
        CtCross,
        CtHand,
        CtIbeam,
        CtWait,
        CtHelp,
        CtEastresize,
        CtNorthresize,
        CtNortheastresize,
        CtNorthwestresize,
        CtSouthresize,
        CtSoutheastresize,
        CtSouthwestresize,
        CtWestresize,
        CtNorthsouthresize,
        CtEastwestresize,
        CtNortheastsouthwestresize,
        CtNorthwestsoutheastresize,
        CtColumnresize,
        CtRowresize,
        CtMiddlepanning,
        CtEastpanning,
        CtNorthpanning,
        CtNortheastpanning,
        CtNorthwestpanning,
        CtSouthpanning,
        CtSoutheastpanning,
        CtSouthwestpanning,
        CtWestpanning,
        CtMove,
        CtVerticaltext,
        CtCell,
        CtContextmenu,
        CtAlias,
        CtProgress,
        CtNodrop,
        CtCopy,
        CtNone,
        CtNotallowed,
        CtZoomin,
        CtZoomout,
        CtGrab,
        CtGrabbing,
        CtCustom,
    }
}
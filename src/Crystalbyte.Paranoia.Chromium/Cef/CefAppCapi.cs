#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [SuppressUnmanagedCodeSecurity]
    public static class CefAppCapi {
        [DllImport(CefAssembly.Name, EntryPoint = "cef_execute_process", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern int CefExecuteProcess(IntPtr args, IntPtr application, IntPtr windowsSandboxInfo);

        [DllImport(CefAssembly.Name, EntryPoint = "cef_initialize", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern int CefInitialize(IntPtr args, IntPtr settings, IntPtr application,
            IntPtr windowsSandboxInfo);

        [DllImport(CefAssembly.Name, EntryPoint = "cef_shutdown", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern void CefShutdown();

        [DllImport(CefAssembly.Name, EntryPoint = "cef_do_message_loop_work",
            CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void CefDoMessageLoopWork();

        [DllImport(CefAssembly.Name, EntryPoint = "cef_run_message_loop", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern void CefRunMessageLoop();

        [DllImport(CefAssembly.Name, EntryPoint = "cef_quit_message_loop", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern void CefQuitMessageLoop();

        [DllImport(CefAssembly.Name, EntryPoint = "cef_set_osmodal_loop", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern void CefSetOsmodalLoop(int osmodalloop);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefApp {
        public CefBase Base;
        public IntPtr OnBeforeCommandLineProcessing;
        public IntPtr OnRegisterCustomSchemes;
        public IntPtr CefCallbackGetResourceBundleHandler;
        public IntPtr CefCallbackGetBrowserProcessHandler;
        public IntPtr CefCallbackGetRenderProcessHandler;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefAppCapiDelegates {
        public delegate void OnBeforeCommandLineProcessingCallback(IntPtr self, IntPtr processType, IntPtr commandLine);

        public delegate void OnRegisterCustomSchemesCallback(IntPtr self, IntPtr registrar);

        public delegate IntPtr GetResourceBundleHandlerCallback(IntPtr self);

        public delegate IntPtr GetBrowserProcessHandlerCallback(IntPtr self);

        public delegate IntPtr GetRenderProcessHandlerCallback(IntPtr self);
    }
}
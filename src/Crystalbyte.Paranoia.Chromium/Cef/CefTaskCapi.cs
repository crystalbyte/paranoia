#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Security;
using Crystalbyte.Paranoia.Cef.Internal;

#endregion

namespace Crystalbyte.Paranoia.Cef {
    [SuppressUnmanagedCodeSecurity]
    public static class CefTaskCapi {
        [DllImport(CefAssembly.Name, EntryPoint = "cef_task_runner_get_for_current_thread",
            CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern IntPtr CefTaskRunnerGetForCurrentThread();

        [DllImport(CefAssembly.Name, EntryPoint = "cef_task_runner_get_for_thread",
            CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern IntPtr CefTaskRunnerGetForThread(CefThreadId threadid);

        [DllImport(CefAssembly.Name, EntryPoint = "cef_currently_on", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern int CefCurrentlyOn(CefThreadId threadid);

        [DllImport(CefAssembly.Name, EntryPoint = "cef_post_task", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern int CefPostTask(CefThreadId threadid, IntPtr task);

        [DllImport(CefAssembly.Name, EntryPoint = "cef_post_delayed_task", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern int CefPostDelayedTask(CefThreadId threadid, IntPtr task, long delayMs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefTask {
        public CefBase Base;
        public IntPtr Execute;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CefTaskRunner {
        public CefBase Base;
        public IntPtr IsSame;
        public IntPtr BelongsToCurrentThread;
        public IntPtr BelongsToThread;
        public IntPtr PostTask;
        public IntPtr PostDelayedTask;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class CefTaskCapiDelegates {
        public delegate void ExecuteCallback(IntPtr self);

        public delegate int IsSameCallback4(IntPtr self, IntPtr that);

        public delegate int BelongsToCurrentThreadCallback(IntPtr self);

        public delegate int BelongsToThreadCallback(IntPtr self, CefThreadId threadid);

        public delegate int PostTaskCallback(IntPtr self, IntPtr task);

        public delegate int PostDelayedTaskCallback(IntPtr self, IntPtr task, long delayMs);
    }
}
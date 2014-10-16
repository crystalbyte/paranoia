using System;
using System.Runtime.InteropServices;

namespace Crystalbyte.Paranoia.Automation {

    [ComImport]
    [ComVisible(false)]
    [Guid("00000001-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IClassFactory {
        [PreserveSig]
        UInt32 CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject);

        [PreserveSig]
        UInt32 LockServer(bool fLock);
    }
}

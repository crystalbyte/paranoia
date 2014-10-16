using System;
using System.Runtime.InteropServices;

namespace Crystalbyte.Paranoia.Automation {

    /// <summary>
    /// This class serves as the class factory for the Application object.
    /// Microsoft: http://support.microsoft.com/kb/977996
    /// CodeProject: http://www.codeproject.com/Articles/12579/Building-COM-Servers-in-NET
    /// </summary>
    [ComVisible(true)]
    [ProgId(ProgId), Guid(ClassId)]
    [ClassInterface(ClassInterfaceType.None)]
    public sealed class ApplicationClassFactory : IClassFactory {

        #region Private Fields

        private int? _cookie;
        private readonly IComServer _server;

        #endregion

        #region Construction

        public ApplicationClassFactory(IComServer server) {
            _server = server;
        }

        #endregion

        #region COM Registration

        public const string ProgId = "Paranoia.Application";
        public const string ClassId = "AA1CC4EE-2EB6-4521-940B-5B4C56C46CB0";

        #endregion

        #region Methods

        public void Register() {
            const RegistrationClassContext flags = RegistrationClassContext.LocalServer;

            var services = new RegistrationServices();
            _cookie = services.RegisterTypeForComClients(typeof(ApplicationClassFactory),
                flags, RegistrationConnectionType.MultipleUse);
        }

        public void Revoke() {
            if (_cookie == null) {
                return;
            }
            var services = new RegistrationServices();
            services.UnregisterTypeForComClients(_cookie.Value);
        }

        #endregion

        #region Implementation of IClassFactory


        public UInt32 CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject) {
            try {
                if (riid != Marshal.GenerateGuidForType(typeof(Application))
                    && riid != IID_IDispatch && riid != IID_IUnknown) {
                    throw new COMException(
                        "The object that ppvObject points to does not support the interface identified by riid.",
                        unchecked((int)E_NOINTERFACE));
                }

                ppvObject = Marshal.GetComInterfaceForObject(new Application(_server), typeof(IApplication));
                return S_OK;
            } catch (COMException) {
                ppvObject = IntPtr.Zero;
                return E_NOINTERFACE;
            }
        }

        public UInt32 LockServer(bool fLock) {
            try {
                if (fLock) {
                    _server.IncrementServerLock();
                } else {
                    _server.IncrementServerLock();
                }
                return S_OK;
            } catch (Exception) {
                return E_FAIL;
            }
        }

        #endregion

        #region Native Support

        // ReSharper disable InconsistentNaming
        public static Guid IID_IUnknown = new Guid("{00000000-0000-0000-C000-000000000046}");
        public static Guid IID_IDispatch = new Guid("{00020400-0000-0000-C000-000000000046}");

        private const UInt32 S_OK = 0;
        private const UInt32 E_NOINTERFACE = 0x80004002;
        private const UInt32 E_FAIL = 0x80004005;
        // ReSharper restore InconsistentNaming

        #endregion
    }
}

using System;
using System.Runtime.InteropServices;

namespace Crystalbyte.Paranoia.Automation {
    
    [ComVisible(true)]
    [ProgId(ProgId), Guid(ClassId)]
    [ClassInterface(ClassInterfaceType.None)]
    public sealed class ApplicationClassFactory : IClassFactory {

        #region Private Fields

        private static int? _cookie;

        #endregion

        #region COM Registration

        public const string ProgId = "Paranoia.Application";
        public const string ClassId = "AA1CC4EE-2EB6-4521-940B-5B4C56C46CB0";

        #endregion

        #region Methods

        public static void Register() {
            const RegistrationClassContext flags = RegistrationClassContext.LocalServer;

            var services = new RegistrationServices();
            _cookie = services.RegisterTypeForComClients(typeof(ApplicationClassFactory),
                flags, RegistrationConnectionType.MultipleUse);
        }

        public static void Revoke() {
            if (_cookie == null) {
                return;
            }
            var services = new RegistrationServices();
            services.UnregisterTypeForComClients(_cookie.Value);
        }

        #endregion

        #region Implementation of IClassFactory

        public int CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject) {
            throw new NotImplementedException();
        }

        public int LockServer(bool fLock) {
            throw new NotImplementedException();
        }

        #endregion
    }
}

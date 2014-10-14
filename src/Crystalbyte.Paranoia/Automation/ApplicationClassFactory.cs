using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media;

namespace Crystalbyte.Paranoia.Automation {
    
    [ComVisible(true)]
    [ProgId(ProgId), Guid(ClassId)]
    [ClassInterface(ClassInterfaceType.None)]
    public sealed class ApplicationClassFactory : IClassFactory {

        #region Private Fields

        private int? _cookie;

        #endregion

        #region COM Registration

        internal const string ProgId = "Paranoia.Application";
        internal const string ClassId = "AA1CC4EE-2EB6-4521-940B-5B4C56C46CB0";

        #endregion

        #region Methods

        public void Publish() {
            const RegistrationClassContext flags = RegistrationClassContext.InProcessServer;

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

        public int CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject) {
            throw new NotImplementedException();
        }

        public int LockServer(bool fLock) {
            throw new NotImplementedException();
        }

        #endregion
    }
}

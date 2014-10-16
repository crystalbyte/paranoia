using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Automation {

    [ComVisible(true)]
    [Guid(Application.InterfaceId)]
    public interface IApplication {
        void OpenFile(string path);
    }
}

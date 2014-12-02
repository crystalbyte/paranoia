using System.Runtime.InteropServices;

namespace Crystalbyte.Paranoia.Automation {

    [ComVisible(true)]
    [Guid(Application.InterfaceId)]
    public interface IApplication {
        void OpenFile(string path);
    }
}

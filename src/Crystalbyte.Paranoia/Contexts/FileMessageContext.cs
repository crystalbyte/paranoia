using System.IO;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    public sealed class FileMessageContext : NotificationObject {
        private readonly FileSystemInfo _file;

        public FileMessageContext(FileSystemInfo file) {
            _file = file;
        }

        public string FullName {
            get { return _file.FullName; }
        }

        internal Task LoadAsync() {
            throw new System.NotImplementedException();
        }
    }
}

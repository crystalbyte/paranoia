using System.Threading.Tasks;
using System.Windows;

namespace Crystalbyte.Paranoia {
    public sealed class StreamAttachmentContext : AttachmentBase {

        #region Private Fields

        private readonly string _filename;
        private readonly byte[] _bytes;

        #endregion

        #region Construction

        public StreamAttachmentContext(string filename, byte[] bytes) {
            _filename = filename;
            _bytes = bytes;
        }

        #endregion

        #region Class Overrides

        public override string Filename {
            get { return _filename; }
        }

        public override byte[] GetBytes() {
            return _bytes;
        }

        #endregion
    }
}

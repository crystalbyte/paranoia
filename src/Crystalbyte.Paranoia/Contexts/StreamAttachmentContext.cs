using System.Threading.Tasks;
using System.Windows;

namespace Crystalbyte.Paranoia {
    public sealed class StreamAttachmentContext : AttachmentBase {

        #region Private Fields

        private readonly string _name;
        private readonly byte[] _bytes;

        #endregion

        #region Construction

        public StreamAttachmentContext(string name, byte[] bytes) {
            _name = name;
            _bytes = bytes;
        }

        #endregion

        #region Class Overrides

        public override string Name {
            get { return _name; }
        }

        public override byte[] GetBytes() {
            return _bytes;
        }

        #endregion
    }
}

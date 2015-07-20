using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    public abstract class AttachmentBase {

        #region Abstract Methods & Properties

        public abstract string Filename { get; }

        public abstract byte[] GetBytes();

        #endregion

        #region Methods

        public Task<byte[]> GetBytesAsync() {
            return Task.Run(() => GetBytes());
        }

        #endregion

        #region Properties

        /// <summary>
        /// The property Bytes should only be used to be bound by XAML and must be called asynchronously.
        /// </summary>
        public byte[] Bytes {
            get { return GetBytes(); }
        }

        public bool IsImage {
            get {
                return Regex.IsMatch(Filename, ".jpg|.png|.jpeg|.tiff|.gif", RegexOptions.IgnoreCase);
            }
        }

        #endregion
    }
}

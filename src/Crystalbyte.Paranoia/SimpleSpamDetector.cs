using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    public sealed class SimpleSpamDetector {

        #region Private Fields

        private readonly string _message;

        #endregion

        #region Construction

        public SimpleSpamDetector(string message) {
            _message = message;
        }

        #endregion

        #region Methods

        public Task<bool> GetIsSpamAsync() {
            return Task.Run(() => _message.ContainsIgnoreCase("unsubscribe"));
        }

        #endregion
    }
}

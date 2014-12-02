using System;
using System.Threading.Tasks;
using NLog;

namespace Crystalbyte.Paranoia {
    public sealed class CreateMailboxContext : NotificationObject {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IMailboxCreator _creator;
        private string _name;

        #endregion

        #region Construction

        public CreateMailboxContext(IMailboxCreator creator) {
            _creator = creator;
        }

        #endregion

        #region Property Declarations

        public string Name {
            get { return _name; }
            set {
                if (_name == value) {
                    return;
                }
                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        #endregion

        #region Method Declarations

        internal async Task CommitAsync() {
            try {
                await _creator.CreateMailboxAsync(Name);
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        #endregion
    }
}

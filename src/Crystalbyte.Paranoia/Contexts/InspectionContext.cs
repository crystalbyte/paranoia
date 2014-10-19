using System.Threading.Tasks;
using Crystalbyte.Paranoia.Mail;

namespace Crystalbyte.Paranoia {
    public abstract class InspectionContext : NotificationObject {

        #region Private Fields

        private string _subject;

        #endregion

        #region Methods

        protected abstract Task<MailMessageReader> GetMailMessageReaderAsync();

        public async Task InitAsync() {
            var reader = await GetMailMessageReaderAsync();
            Subject = reader.Headers.Subject;
        }

        #endregion

        #region Properties

        public string Subject {
            get { return _subject; }
            set {
                if (_subject == value) {
                    return;
                }

                _subject = value;
                RaisePropertyChanged(() => Subject);
            }
        }

        #endregion
    }
}

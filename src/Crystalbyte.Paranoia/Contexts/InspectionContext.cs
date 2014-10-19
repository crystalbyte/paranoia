using System;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Mail;

namespace Crystalbyte.Paranoia {
    public abstract class InspectionContext : NotificationObject {

        #region Private Fields

        private string _subject;
        private string _from;
        private string _to;
        private DateTime _date;

        #endregion

        #region Methods

        protected abstract Task<MailMessageReader> GetMailMessageReaderAsync();

        public async Task InitAsync() {
            var reader = await GetMailMessageReaderAsync();
            Subject = reader.Headers.Subject;
            From = reader.Headers.From.DisplayName;
            Date = reader.Headers.DateSent.ToLocalTime();
            To = string.Join(", ", reader.Headers.To.Select(x => x.DisplayName));
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

        public string From {
            get { return _from; }
            set {
                if (_from == value) {
                    return;
                }

                _from = value;
                RaisePropertyChanged(() => From);
            }
        }

        public string To {
            get { return _to; }
            set {
                if (_to == value) {
                    return;
                }

                _to = value;
                RaisePropertyChanged(() => To);
            }
        }

        public DateTime Date {
            get { return _date; }
            set {
                if (_date == value) {
                    return;
                }
                _date = value;
                RaisePropertyChanged(() => Date);
            }
        }

        #endregion
    }
}

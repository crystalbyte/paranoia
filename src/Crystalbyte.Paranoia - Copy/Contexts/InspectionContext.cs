using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using NLog;

namespace Crystalbyte.Paranoia {
    public abstract class InspectionContext : NotificationObject {

        #region Private Fields

        private string _subject;
        private MailContactContext _from;
        private readonly IList<MailContactContext> _to;
        private readonly IList<MailContactContext> _cc;
        private readonly IList<AttachmentContext> _attachments;
        private DateTime _date;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        protected InspectionContext() {
            _to = new ObservableCollection<MailContactContext>();
            _cc = new ObservableCollection<MailContactContext>();
            _attachments = new ObservableCollection<AttachmentContext>();
        }

        #endregion

        #region Methods

        internal abstract void Reply();

        internal abstract void ReplyAll();

        internal abstract void Forward();

        protected internal abstract Task<MailMessageReader> GetMailMessageReaderAsync();

        public async Task InitAsync() {
            var reader = await GetMailMessageReaderAsync();
            Subject = reader.Headers.Subject;
            Date = reader.Headers.DateSent.ToLocalTime();

            try {
                using (var database = new DatabaseContext()) {
                    var from = await database.MailContacts
                        .FirstOrDefaultAsync(x => x.Address == reader.Headers.From.Address);
                    if (from == null) {
                        from = new MailContactModel {
                            Name = reader.Headers.From.DisplayName,
                            Address = reader.Headers.From.Address
                        };

                        database.MailContacts.Add(from);
                    }

                    _from = new MailContactContext(from);

                    foreach (var value in reader.Headers.To) {
                        var v = value;
                        var contact = await database.MailContacts
                            .FirstOrDefaultAsync(x => x.Address == v.Address);
                        if (contact == null) {
                            contact = new MailContactModel {
                                Name = v.DisplayName,
                                Address = v.Address
                            };

                            database.MailContacts.Add(contact);
                        }
                        _to.Add(new MailContactContext(contact));
                    }

                    foreach (var value in reader.Headers.Cc) {
                        var v = value;
                        var contact = await database.MailContacts
                            .FirstOrDefaultAsync(x => x.Address == v.Address);
                        if (contact == null) {
                            contact = new MailContactModel {
                                Name = v.DisplayName,
                                Address = v.Address
                            };

                            database.MailContacts.Add(contact);
                        }
                        _cc.Add(new MailContactContext(contact));
                    }

                    foreach (var attachment in reader.FindAllAttachments()) {
                        _attachments.Add(new AttachmentContext(attachment));
                    }

                    await database.SaveChangesAsync();
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        #endregion

        #region Properties

        public IEnumerable<AttachmentContext> Attachments {
            get { return _attachments; }
        }

        public bool HasCarbonCopies {
            get { return _cc.Count > 0; }
        }
        public bool HasMultipleRecipients {
            get { return _to.Count > 1; }
        }

        public MailContactContext From {
            get { return _from; }
        }

        public MailContactContext PrimaryTo {
            get { return _to.FirstOrDefault(); }
        }

        public IEnumerable<MailContactContext> To {
            get { return _to; }
        }

        public IEnumerable<MailContactContext> SecondaryTo {
            get { return _to.Skip(1); }
        }

        public IEnumerable<MailContactContext> Cc {
            get { return _cc; }
        }

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

using System.Data.Entity;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;

namespace Crystalbyte.Paranoia {
    public sealed class MessageInspectionContext : InspectionContext {

        #region Private Fields

        private readonly MailMessageContext _message;

        #endregion

        #region Construction

        public MessageInspectionContext(MailMessageContext message) {
            _message = message;
        }

        #endregion

        #region Class Overrides

        protected internal async override Task<MailMessageReader> GetMailMessageReaderAsync() {
            using (var database = new DatabaseContext()) {
                var mime = await database.MimeMessages.FirstOrDefaultAsync(x => x.MessageId == _message.Id);
                return new MailMessageReader(mime.Data);
            }
        }

        internal override void Reply() {
            App.Context.ReplyAsync(_message);
        }

        internal override void ReplyAll() {
            App.Context.ReplyToAllAsync(_message);
        }

        internal override void Forward() {
            App.Context.ForwardAsync(_message);
        }

        #endregion

        #region Properties

        public MailMessageContext Message {
            get { return _message; }
        }

        #endregion
    }
}
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

        internal async override Task ReplyAsync() {
            await App.Context.ReplyAsync(_message);
        }

        internal async override Task ReplyAll() {
            await App.Context.ReplyToAllAsync(_message);
        }

        internal async override Task Forward() {
            await App.Context.ForwardAsync(_message);
        }

        #endregion

        #region Properties

        public MailMessageContext Message {
            get { return _message; }
        }

        #endregion
    }
}
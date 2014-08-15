using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Properties;

namespace Crystalbyte.Paranoia {
    public sealed class OutboxContext : SelectionObject {
        private readonly MailAccountContext _account;
        private int _count;

        public OutboxContext(MailAccountContext account) {
            _account = account;
        }

        public string Name {
            get { return Resources.Outbox; }
        }

        public int Count {
            get { return _count; }
            set {
                if (_count == value) {
                    return;
                }
                _count = value;
                RaisePropertyChanged(() => Count);
            }
        }

        internal async Task CountMessagesAsync() {
            using (var database = new DatabaseContext()) {
                Count = await database.SmtpRequests
                    .Where(x => x.AccountId == _account.Id)
                    .CountAsync();
            }
        }
    }
}

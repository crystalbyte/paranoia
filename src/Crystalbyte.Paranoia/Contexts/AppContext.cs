#region Using directives

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    [Export, Shared]
    public sealed class AppContext {
        public AppContext() {
            Accounts = new ObservableCollection<AccountContext> {
                new AccountContext {
                        Host = "imap.gmail.com",
                        Port = 993,
                        Username = "paranoia.app@gmail.com",
                        Password = "p4r4n014"
                    }
            };
        }

        public ObservableCollection<AccountContext> Accounts { get; set; }

        public IEnumerable<MessageContext> Messages {
            get { return Accounts.SelectMany(x => x.Messages); }
        }

        public async Task SyncAsync() {
            foreach (var account in Accounts) {
                await account.SyncAsync();
            }
            // Update Messages
        }
    }
}

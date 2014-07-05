using System.Collections.ObjectModel;
using System.Composition;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;

namespace Crystalbyte.Paranoia {

    [Export, Shared]
    public sealed class AppContext {

        public AppContext() {
            Accounts = new ObservableCollection<MailAccountContext>();
        }

        public ObservableCollection<MailAccountContext> Accounts { get; set; }

        public async Task RunAsync() {
            await LoadAccountsAsync();
        }

        private async Task LoadAccountsAsync() {
            using (var context = new DatabaseContext()) {
                var accounts = await context.MailAccounts.ToArrayAsync();
                Accounts.AddRange(accounts.Select(x => new MailAccountContext(x)));
            }
        }
    }
}

using System.Collections.ObjectModel;
using System.Composition;
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

        #region Import Directives

        [OnImportsSatisfied]
        public async void OnImportsSatisfied() {
           
        }

        #endregion

        public async Task RunAsync() {
            await LoadAccountsAsync();
        }

        private async Task LoadAccountsAsync() {
            using (var context = new DatabaseContext()) {
                var repo = new Repository(context);
                var accounts = await repo.GetAccountsAsync();
                Accounts.AddRange(accounts.Select(x => new MailAccountContext(x)));
            }
        }
    }
}

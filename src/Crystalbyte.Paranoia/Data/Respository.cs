using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Data {
    internal sealed class Repository : IRepository {

        private readonly DatabaseContext _context;

        public Repository(DatabaseContext context) {
            _context = context;
        }

        #region Implementation of IRepository

        public async Task<MailAccount[]> GetAccountsAsync() {
            return await _context.MailAccounts.ToArrayAsync();
        }

        public async Task<MailContact[]> GetContactsAsync() {
            return await _context.MailContacts.ToArrayAsync();
        }

        public async Task SaveAccountAsync(MailAccount account) {
            _context.MailAccounts.Add(account);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAccountsAsync(IEnumerable<MailAccount> accounts) {
            _context.MailAccounts.AddRange(accounts);
            await _context.SaveChangesAsync();
        }

        public Task<MailAccount> GetAccountAsync(int id) {
            return _context.MailAccounts.FirstOrDefaultAsync(x => x.Id == id);
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Data {
    internal interface IRepository {
        Task<MailAccount[]> GetAccountsAsync();
        Task<MailContact[]> GetContactsAsync();
        Task SaveAccountAsync(MailAccount account);
        Task SaveAccountsAsync(IEnumerable<MailAccount> accounts);
        Task<MailAccount> GetAccountAsync(int id);
    }
}

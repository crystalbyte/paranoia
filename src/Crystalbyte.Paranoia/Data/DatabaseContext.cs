using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Data {
    internal sealed class DatabaseContext : DbContext {
        public DatabaseContext() {
            Database.SetInitializer(new SQLiteDatabaseInitializer<DatabaseContext>());
        }

        public DbSet<MailAccount> MailAccounts { get; set; }
        public DbSet<MailContact> MailContacts { get; set; }
        public DbSet<IsolatedObject> IsolatedObjects { get; set; }
    }
}

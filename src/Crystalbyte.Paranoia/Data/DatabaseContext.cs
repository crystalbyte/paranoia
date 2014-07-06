using System.Data.Entity;

namespace Crystalbyte.Paranoia.Data {
    internal sealed class DatabaseContext : DbContext {
        public DatabaseContext() {
            Database.SetInitializer(new SQLiteDatabaseInitializer<DatabaseContext>());
        }

        public DbSet<MailAccountModel> MailAccounts { get; set; }
        public DbSet<MailContactModel> MailContacts { get; set; }
        public DbSet<MailboxModel> Mailboxes { get; set; }
    }
}

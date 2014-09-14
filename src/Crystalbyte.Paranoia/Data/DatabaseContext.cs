#region Using directives

using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Threading.Tasks;

#endregion

namespace Crystalbyte.Paranoia.Data {
    internal sealed class DatabaseContext : DbContext {
        public DatabaseContext() {
            Database.SetInitializer(new SQLiteDatabaseInitializer<DatabaseContext>());
        }

        public async Task<DbTransaction> BeginTransactionAsync(IsolationLevel level = IsolationLevel.Unspecified) {
            await Database.Connection.OpenAsync();
            return Database.Connection.BeginTransaction(level);
        }

        public DbSet<MailboxModel> Mailboxes { get; set; }
        public DbSet<MailAccountModel> MailAccounts { get; set; }
        public DbSet<MailContactModel> MailContacts { get; set; }
        public DbSet<MailMessageModel> MailMessages { get; set; }
        public DbSet<MimeMessageModel> MimeMessages { get; set; }
        public DbSet<SmtpRequestModel> SmtpRequests { get; set; }
        public DbSet<PublicKeyModel> PublicKeys { get; set; }
    }
}
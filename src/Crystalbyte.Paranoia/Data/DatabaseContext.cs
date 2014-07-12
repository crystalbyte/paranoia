﻿using System.Data.Entity;

namespace Crystalbyte.Paranoia.Data {
    internal sealed class DatabaseContext : DbContext {
        public DatabaseContext() {
            Database.SetInitializer(new SQLiteDatabaseInitializer<DatabaseContext>());
        }

        public DbSet<MailboxModel> Mailboxes { get; set; }
        public DbSet<MailAccountModel> MailAccounts { get; set; }
        public DbSet<MailContactModel> MailContacts { get; set; }
        public DbSet<MailMessageModel> MailMessages { get; set; }
        public DbSet<MimeMessageModel> MimeMessages { get; set; }
    }
}
#region Using directives

using System;
using System.Composition;
using System.Data.Entity;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Crystalbyte.Paranoia.Models;
using System.Diagnostics;

#endregion

namespace Crystalbyte.Paranoia.Data {
    public sealed class StorageContext : DbContext {
        private const string Filename = "storage.sdf";

        public StorageContext()
            : base(Filename) { }

        public DbSet<Contact> Contacts { get; set; }

        public DbSet<Identity> Identities { get; set; }

        public DbSet<ImapAccount> ImapAccounts { get; set; }

        public DbSet<Mailbox> Mailboxes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            modelBuilder.Entity<Identity>()
                        .HasRequired(s => s.ImapAccount)
                        .WithRequiredPrincipal(a => a.Identity)
                        .WillCascadeOnDelete();

            modelBuilder.Entity<Identity>()
                        .HasRequired(s => s.SmtpAccount)
                        .WithRequiredPrincipal(a => a.Identity)
                        .WillCascadeOnDelete();
        }

        private static string StorageDirectory {
            get {
                var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var directory = Path.Combine(roaming, App.Name, "Data");
                return directory;
            }
        }

        public static Task<bool> IsDatabaseCreatedAsync() {
            return Task<bool>.Factory.StartNew(() => {
                if (!Directory.Exists(StorageDirectory)) {
                    return false;
                }

                var file = Path.Combine(StorageDirectory, Filename);
                return File.Exists(file);
            });
        }

        public async Task InitAsync() {
            OverrideDataDirectory();
            var created = await IsDatabaseCreatedAsync();
            if (created) {
                return;
            }
            await CreateDatabaseAsync();
        }

        private static void OverrideDataDirectory() {
            // http://stackoverflow.com/questions/1409358/ado-net-datadirectory-where-is-this-documented
            AppDomain.CurrentDomain.SetData("DataDirectory", StorageDirectory);
        }

        private Task CreateDatabaseAsync() {
            return Task.Factory.StartNew(() => {
                var directory = StorageDirectory;

                if (!Directory.Exists(directory)) {
                    Directory.CreateDirectory(directory);
                }

                Database.Create();
            });
        }
    }
}
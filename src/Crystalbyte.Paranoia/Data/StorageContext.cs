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
    [Export, Shared]
    public sealed class StorageContext : DbContext {
        private const string Filename = "storage.sdf";

        public static StorageContext Current { get; set; }

        public StorageContext()
            : base(Filename) {
            if (Current != null) {
                throw new InvalidOperationException("One does not simply call StorageContext twice!");
            }
            Current = this;
        }

        public DbSet<Identity> Identities { get; set; }
        
        private static string StorageDirectory {
            get {
                var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var directory = Path.Combine(roaming, App.Name, "Data");
                return directory;
            }
        }

        public Task<bool> IsDatabaseCreatedAsync() {
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
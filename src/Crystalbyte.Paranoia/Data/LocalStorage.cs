using System.Data.Entity;
using System.Data.EntityClient;
using Crystalbyte.Paranoia.Models;
using System;
using System.Composition;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Crystalbyte.Paranoia.Data {

    [Export, Shared]
    public sealed class LocalStorage {

        private readonly Entities _context;
        private const string ApplicationName = "Paranoia";
        private const string DatabaseFilename = "Storage.sdf";

        public LocalStorage() {
            _context = new Entities();
        }

        public Task InsertAsync(Identity identity) {
            return Task.Factory.StartNew(() => {
                _context.Identities.Add(identity);
                _context.SaveChanges();
            });
        }

        public Task<DbSet<Identity>> QueryIdentitiesAsync() {
            return Task.Factory.StartNew(() => _context.Identities);
        }

        public Task<DbSet<ImapAccount>> QueryImapAccountsAsync() {
            return Task.Factory.StartNew(() => _context.ImapAccounts);
        }

        public Task<DbSet<SmtpAccount>> QuerySmtpAccountsAsync() {
            return Task.Factory.StartNew(() => _context.SmtpAccounts);
        }

        private static string DataDirectory {
            get {
                var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var directory = Path.Combine(roaming, ApplicationName, "Data");
                return directory;
            }
        }

        public Task<bool> CheckIsCreatedAsync() {
            return Task<bool>.Factory.StartNew(() => {

                if (!Directory.Exists(DataDirectory)) {
                    return false;
                }

                var file = Path.Combine(DataDirectory, DatabaseFilename);
                return File.Exists(file);
            });
        }

        public async Task InitAsync() {
            ApplyDataDirectory();
            var created = await CheckIsCreatedAsync();
            if (created) {
                return;
            }
            await SetupDatabaseAsync();
        }

        private static void ApplyDataDirectory() {
            AppDomain.CurrentDomain.SetData("DataDirectory", DataDirectory);
        }

        private static Task SetupDatabaseAsync() {
            return Task.Factory.StartNew(() => {

                var directory = DataDirectory;
                if (!Directory.Exists(directory)) {
                    Directory.CreateDirectory(directory);
                }

                var file = Path.Combine(directory, DatabaseFilename);
                var info = Application.GetResourceStream(new Uri("Data/Storage.sdf", UriKind.RelativeOrAbsolute));
                if (info == null) {
                    throw new Exception(string.Format("Resource missing: {0}", "/Data/Storage.sdf"));
                }

                using (var reader = new BinaryReader(info.Stream)) {
                    using (var writer = new BinaryWriter(File.Create(file))) {
                        while (true) {
                            var bytes = reader.ReadBytes(4096);
                            if (bytes.Length == 0) {
                                break;
                            }
                            writer.Write(bytes);
                        }
                    }
                }
            });
        }
    }
}

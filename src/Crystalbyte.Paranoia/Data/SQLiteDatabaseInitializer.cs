using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Crystalbyte.Paranoia.Data {
    public sealed class StorageInitializer<T> : IDatabaseInitializer<T> where T : DbContext {
        public void InitializeDatabase(T context) {
            var c = context.Database.Connection.ConnectionString;
            var reader = new SQLiteConnectionStringReader(c);
            var path = reader.DataSource;
            if (File.Exists(path))
                return;

            CreateDatabase(path);
            CreateSchema(context);
        }

        private static void CreateSchema(T context) {
            var models = typeof(T).GetProperties()
                .Where(x => x.PropertyType.IsGenericType)
                .Where(x => x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(x => x.PropertyType.GetGenericArguments().First())
                .ToArray();

            foreach (var model in models) {
                var analyzer = new SQLiteModelAnalyzer(model);
                var script = analyzer.GetTableCreateScript();
            }


        }

        private static void CreateDatabase(string path) {
            var info = Application.GetResourceStream(new Uri("Resources/storage.db", UriKind.Relative));
            if (info == null) {
                throw new Exception("Embedded database not found.");
            }

            using (var reader = new BinaryReader(info.Stream)) {
                var bytes = reader.ReadBytes(2048);
                File.WriteAllBytes(path, bytes);
            }
        }
    }
}

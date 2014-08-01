#region Using directives

using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using Crystalbyte.Paranoia.Properties;

#endregion

namespace Crystalbyte.Paranoia.Data {
    internal sealed class SQLiteDatabaseInitializer<T> : IDatabaseInitializer<T> where T : DbContext {
        public void InitializeDatabase(T context) {
            var c = context.Database.Connection.ConnectionString;
            var reader = new SQLiteConnectionStringReader(c);
            var path = reader.DataSource;
            if (File.Exists(path))
                return;

            var info = new FileInfo(path);
            if (info.Directory != null && !info.Directory.Exists) {
                info.Directory.Create();
            }

            CreateDatabase(path);
            CreateSchema(context);
        }

        private static void CreateSchema(T context) {
            var models = typeof (T).GetProperties()
                .Where(x => x.PropertyType.IsGenericType)
                .Where(x => x.PropertyType.GetGenericTypeDefinition() == typeof (DbSet<>))
                .Select(x => x.PropertyType.GetGenericArguments().First())
                .ToArray();

            EnforceForeignKeys(context);
            foreach (var script in models
                .Select(model => new SQLiteModelAnalyzer(model))
                .Select(analyzer => analyzer.GetTableCreateScript())) {
                context.Database.ExecuteSqlCommand(script);
            }
        }

        private static void EnforceForeignKeys(T context) {
            const string command = "PRAGMA foreign_keys = \"1\";";
            context.Database.ExecuteSqlCommand(command);
        }

        private static void CreateDatabase(string path) {
            var info = Application.GetResourceStream(new Uri("Resources/storage.db", UriKind.Relative));
            if (info == null) {
                throw new Exception(Resources.DatabaseImageNotFound);
            }

            using (var reader = new BinaryReader(info.Stream)) {
                var bytes = reader.ReadBytes(2048);
                File.WriteAllBytes(path, bytes);
            }
        }
    }
}
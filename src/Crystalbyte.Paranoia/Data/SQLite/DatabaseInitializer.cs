#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia
// 
// Crystalbyte.Paranoia is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License.
// 
// Foobar is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Using Directives

using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using Crystalbyte.Paranoia.Properties;

#endregion

namespace Crystalbyte.Paranoia.Data.SQLite {
    internal sealed class DatabaseInitializer<T> : IDatabaseInitializer<T> where T : DbContext {
        public void InitializeDatabase(T context) {
            var c = context.Database.Connection.ConnectionString;
            var reader = new ConnectionStringReader(c);
            var path = reader.DataSource;
            if (File.Exists(path)) {
                return;
            }

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

            context.EnableForeignKeys();

            var analyzers = models.Select(model => new ModelAnalyzer(model));
            foreach (var analyzer in analyzers) {
                var tableScript = analyzer.GetTableCreateScript();
                context.Database.ExecuteSqlCommand(tableScript);

                string indexScript;
                var hasIndices = analyzer.TryGetIndexCreateScript(out indexScript);
                if (hasIndices) {
                    context.Database.ExecuteSqlCommand(indexScript);
                }
            }
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
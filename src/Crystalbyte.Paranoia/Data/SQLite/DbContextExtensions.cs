using System.Data.Entity;

namespace Crystalbyte.Paranoia.Data.SQLite {
    internal static class DbContextExtensions {
        public static void EnableForeignKeys(this DbContext context) {
            context.Database.ExecuteSqlCommand("PRAGMA foreign_keys = ON;");
        }

        public static void Connect(this DbContext context) {
            context.Database.Connection.Open();
        }
    }
}

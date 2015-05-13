using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Crystalbyte.Paranoia.Data.SQLite {
    internal static class DbContextExtensions {
        public static void EnableForeignKeys(this DbContext context) {
            context.Database.ExecuteSqlCommand("PRAGMA foreign_keys = ON;");
        }

        public static void Connect(this DbContext context) {
            context.Database.Connection.Open();
        }

        public static async Task SaveChangesAsync(this DbContext context, OptimisticConcurrencyStrategy strategy = OptimisticConcurrencyStrategy.ClientWins) {
            // Handle Optimistic Concurrency.
            // https://msdn.microsoft.com/en-us/data/jj592904.aspx?f=255&MSPPError=-2147217396
            while (true) {
                try {
                    await context.SaveChangesAsync();
                    break;
                } catch (DbUpdateConcurrencyException ex) {
                    if (strategy == OptimisticConcurrencyStrategy.DatabaseWins) {
                        ex.Entries.ForEach(x => x.Reload());
                    } else {
                        ex.Entries.ForEach(x => x.OriginalValues.SetValues(x.GetDatabaseValues()));
                    }
                }
            }
        }
    }
}

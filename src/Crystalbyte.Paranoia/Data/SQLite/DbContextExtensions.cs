using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Data.SQLite {
    internal static class DbContextExtensions {
        public static Task EnableForeignKeysAsync(this DbContext context) {
            return context.Database.ExecuteSqlCommandAsync("PRAGMA foreign_keys = ON;");
        }

        public static Task ConnectAsync(this DbContext context) {
            return context.Database.Connection.OpenAsync();
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

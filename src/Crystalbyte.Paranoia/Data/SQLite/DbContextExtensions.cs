using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;
using NLog;

namespace Crystalbyte.Paranoia.Data.SQLite {
    internal static class DbContextExtensions {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static Task EnableForeignKeysAsync(this DbContext context) {
            return context.Database.ExecuteSqlCommandAsync("PRAGMA foreign_keys = ON;");
        }

        public static Task EnableAutomaticIndexer(this DbContext context) {
            return context.Database.ExecuteSqlCommandAsync("PRAGMA automatic_index = ON;");
        }

        public static Task DisableAutomaticIndexer(this DbContext context) {
            return context.Database.ExecuteSqlCommandAsync("PRAGMA automatic_index = OFF;");
        }

        public static Task OpenAsync(this DbContext context) {
            return context.Database.Connection.OpenAsync();
        }

        public static void Close(this DbContext context) {
            context.Database.Connection.Close();
        }

        public static async Task SaveChangesAsync(this DbContext context, OptimisticConcurrencyStrategy strategy = OptimisticConcurrencyStrategy.ClientWins) {
            // Handle Optimistic Concurrency.
            // https://msdn.microsoft.com/en-us/data/jj592904.aspx?f=255&MSPPError=-2147217396
            while (true) {
                try {
                    var t1 = Environment.TickCount & Int32.MaxValue;

                    await context.SaveChangesAsync();

                    var t2 = Environment.TickCount & Int32.MaxValue;
                    Logger.Debug("DbContextExtensions::SaveChangesAsync duration: {0} seconds.", (t2 - t1) / 1000);

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

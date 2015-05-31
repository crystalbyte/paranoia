using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using NLog;

namespace Crystalbyte.Paranoia {
    public sealed class MailNavigationContext : NavigationContext {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Methods

        public async Task CountGlobalUnseenAsync() {
            var start = Environment.TickCount & Int32.MaxValue;

            Counter = await Task.Run(async () => {
                using (var context = new DatabaseContext()) {
                    var all = context.MailMessages.CountAsync();

                    var seen = context.MailMessageFlags
                        .Where(x => x.Value == MailMessageFlags.Seen)
                        .CountAsync();

                    return (await all) - (await seen);
                }
            });

            var finish = Environment.TickCount & Int32.MaxValue;
            if (finish - start > 1000) {
                Logger.Warn(Resources.QueryPerformanceTemplate, finish - start / 1000.0f);
            }
        }

        #endregion
    }
}

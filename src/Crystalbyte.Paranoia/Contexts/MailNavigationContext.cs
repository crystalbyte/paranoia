﻿using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;

namespace Crystalbyte.Paranoia {
    public sealed class MailNavigationContext : NavigationContext {

        public async Task RefreshAsync() {
            Counter = await Task.Run(() => {
                using (var context = new DatabaseContext()) {
                    return context.MailMessages.Count(x => x.Flags.All(y => y.Value != MailMessageFlags.Seen));
                }
            });
        }
    }
}
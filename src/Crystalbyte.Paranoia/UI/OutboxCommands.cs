using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI {
    public static class OutboxCommands {
        public static RoutedCommand Delete = new RoutedUICommand(string.Empty, "Delete", typeof(Page));
    }
}

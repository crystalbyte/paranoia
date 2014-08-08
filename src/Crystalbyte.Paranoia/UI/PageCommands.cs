using System.Windows.Controls;
using System.Windows.Input;
using Crystalbyte.Paranoia.Properties;

namespace Crystalbyte.Paranoia.UI {
    public static class PageCommands {
        public static RoutedCommand Commit = new RoutedUICommand(Resources.Update, "Commit", typeof(Page));
        public static RoutedCommand Cancel = new RoutedUICommand(Resources.Cancel, "Cancel", typeof(Page));
    }
}

using System.Windows.Controls;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI {
    public static class MailboxSelectionCommands {
        public static RoutedCommand Select = new RoutedUICommand(string.Empty, "Select", typeof(Page));
        public static RoutedCommand Cancel = new RoutedUICommand(string.Empty, "Cancel", typeof(Page));
        public static RoutedCommand Commit = new RoutedUICommand(string.Empty, "Commit", typeof(Page));
    }
}

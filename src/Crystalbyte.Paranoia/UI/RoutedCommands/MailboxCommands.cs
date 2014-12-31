#region Using directives

using System.Windows.Input;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public static class MailboxCommands {
        public static RoutedCommand SelectRole = new RoutedCommand("SelectRole", typeof(MailboxCommands));
        public static RoutedCommand Browse = new RoutedCommand("Browse", typeof(MailboxCommands));
        public static RoutedCommand Create = new RoutedCommand("Create", typeof(MailboxCommands));
        public static RoutedCommand Delete = new RoutedCommand("Delete", typeof(MailboxCommands));
        public static RoutedCommand Sync = new RoutedCommand("Sync", typeof(MailboxCommands));
    }
}
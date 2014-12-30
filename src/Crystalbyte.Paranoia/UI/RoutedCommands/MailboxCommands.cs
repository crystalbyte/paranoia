#region Using directives

using System.Windows.Input;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public static class MailboxCommands {
        public static RoutedCommand SelectRole = new RoutedCommand("SelectRole", typeof(MailboxCommands));
        public static RoutedCommand Browse = new RoutedCommand("Browse", typeof(MailboxCommands));
    }
}
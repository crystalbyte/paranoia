using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI {
    public static class MessagingCommands {
        public static RoutedCommand Compose = new RoutedUICommand(string.Empty, "Compose", typeof(ApplicationCommands));
        public static RoutedCommand Reply = new RoutedUICommand(string.Empty, "Reply", typeof(ApplicationCommands));
        public static RoutedCommand ReplyAll = new RoutedUICommand(string.Empty, "ReplyAll", typeof(ApplicationCommands));
        public static RoutedCommand Forward = new RoutedUICommand(string.Empty, "Forward", typeof(ApplicationCommands));
        public static RoutedCommand Resume = new RoutedUICommand(string.Empty, "Resume", typeof(ApplicationCommands));
        public static RoutedCommand Inspect = new RoutedUICommand(string.Empty, "Inspect", typeof(ApplicationCommands));
    }
}

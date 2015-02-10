using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI {
    public static class MessageCommands {
        public static RoutedCommand Compose = new RoutedUICommand(string.Empty, "Compose", typeof(MessageCommands));
        public static RoutedCommand Reply = new RoutedUICommand(string.Empty, "Reply", typeof(MessageCommands));
        public static RoutedCommand ReplyAll = new RoutedUICommand(string.Empty, "ReplyAll", typeof(MessageCommands));
        public static RoutedCommand Forward = new RoutedUICommand(string.Empty, "Forward", typeof(MessageCommands));
        public static RoutedCommand Resume = new RoutedUICommand(string.Empty, "Resume", typeof(MessageCommands));
        public static RoutedCommand Inspect = new RoutedUICommand(string.Empty, "Inspect", typeof(MessageCommands));
    }
}

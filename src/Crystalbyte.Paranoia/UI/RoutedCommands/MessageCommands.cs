using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI {
    public static class MessageCommands {
        public static RoutedCommand Compose = new RoutedCommand("Compose", typeof(MessageCommands));
        public static RoutedCommand Reply = new RoutedCommand("Reply", typeof(MessageCommands));
        public static RoutedCommand ReplyAll = new RoutedCommand("ReplyAll", typeof(MessageCommands));
        public static RoutedCommand Forward = new RoutedCommand("Forward", typeof(MessageCommands));
        public static RoutedCommand Resume = new RoutedCommand("Resume", typeof(MessageCommands));
        public static RoutedCommand Inspect = new RoutedCommand("Inspect", typeof(MessageCommands));
        public static RoutedCommand QuickSearch = new RoutedCommand("QuickSearch", typeof(MessageCommands));
    }
}

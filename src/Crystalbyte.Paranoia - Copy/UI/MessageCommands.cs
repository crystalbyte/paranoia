using System.Windows.Controls;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI {
    public static class MessageCommands {
        public static RoutedCommand Compose = new RoutedUICommand(string.Empty, "Compose", typeof(Page));
        public static RoutedCommand Reply = new RoutedUICommand(string.Empty, "Reply", typeof(Page));
        public static RoutedCommand ReplyAll = new RoutedUICommand(string.Empty, "ReplyAll", typeof(Page));
        public static RoutedCommand Forward = new RoutedUICommand(string.Empty, "Forward", typeof(Page));
        public static RoutedCommand Resume = new RoutedUICommand(string.Empty, "Resume", typeof(Page));
        public static RoutedCommand Inspect = new RoutedUICommand(string.Empty, "Inspect", typeof(Page));
    }
}

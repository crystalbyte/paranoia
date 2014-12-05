using System.Windows;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI {
    public static class MessageCommands {
        public static RoutedCommand Compose = new RoutedUICommand(string.Empty, "Compose", typeof(Window));
        public static RoutedCommand Reply = new RoutedUICommand(string.Empty, "Reply", typeof(Window));
        public static RoutedCommand ReplyAll = new RoutedUICommand(string.Empty, "ReplyAll", typeof(Window));
        public static RoutedCommand Forward = new RoutedUICommand(string.Empty, "Forward", typeof(Window));
        public static RoutedCommand Resume = new RoutedUICommand(string.Empty, "Resume", typeof(Window));
        public static RoutedCommand Inspect = new RoutedUICommand(string.Empty, "Inspect", typeof(Window));
    }
}

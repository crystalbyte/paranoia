using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI {
    public static class AppCommands {
        public static RoutedCommand Settings = new RoutedCommand("Settings", typeof(AppCommands));
        public static RoutedCommand JumpToContact = new RoutedCommand("JumpToContact", typeof(AppCommands));
    }
}

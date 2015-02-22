using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI {
    public static class EditingCommands {
        public static RoutedCommand InsertAttachment = new RoutedCommand("InsertAttachment", typeof(EditingCommands));
        public static RoutedCommand InsertLink = new RoutedCommand("InsertLink", typeof(EditingCommands));
        public static RoutedCommand InsertPicture = new RoutedCommand("InsertPicture", typeof(EditingCommands));
        public static RoutedCommand ToggleStrikethrough = new RoutedCommand("ToggleStrikethrough", typeof(EditingCommands));
    }
}

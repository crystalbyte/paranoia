using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI {
    public static class CompositionCommands {
        public static RoutedCommand Attachment = new RoutedCommand("Attachment", typeof(CompositionCommands));
        public static RoutedCommand Link = new RoutedCommand("Link", typeof(CompositionCommands));
    }
}

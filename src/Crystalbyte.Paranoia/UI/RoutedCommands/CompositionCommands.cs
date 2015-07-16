using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI {
    public static class CompositionCommands {
        public static RoutedCommand SendToOutbox = new RoutedCommand("SendToOutbox", typeof(CompositionCommands));
        public static RoutedCommand SaveAsDraft = new RoutedCommand("SaveAsDraft", typeof(CompositionCommands));
    }
}

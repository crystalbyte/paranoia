using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI {
    public sealed class AutoCompleteBoxCommands {

        private static readonly RoutedCommand SelectCommand = new RoutedCommand();

        public static RoutedCommand Select {
            get { return SelectCommand; }
        }
    }
}

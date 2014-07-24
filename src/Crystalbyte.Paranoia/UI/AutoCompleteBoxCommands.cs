using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI {
    public sealed class AutoCompleteBoxCommands {

        private static readonly RoutedCommand DeleteCommand = new RoutedCommand();

        public static RoutedCommand Delete {
            get { return DeleteCommand; }
        }

        private static readonly RoutedCommand AutoCompleteCommand = new RoutedCommand();

        public static RoutedCommand AutoComplete {
            get { return AutoCompleteCommand; }
        }

        private static readonly RoutedCommand CloseAutoCompleteCommand = new RoutedCommand();

        public static RoutedCommand CloseAutoComplete {
            get { return CloseAutoCompleteCommand; }
        }
    }
}

#region Using directives

using System.Windows.Input;
using Crystalbyte.Paranoia.UI.Properties;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public static class SuggestionBoxCommands {
        public static RoutedUICommand Select =
            new RoutedUICommand(Resources.SelectCommand, "Select", typeof(SuggestionBox));
    }
}
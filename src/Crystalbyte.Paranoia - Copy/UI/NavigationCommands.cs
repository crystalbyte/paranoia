#region Using directives

using System.Windows.Controls;
using System.Windows.Input;
using Crystalbyte.Paranoia.Properties;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public static class NavigationCommands {
        public static RoutedCommand Continue = new RoutedUICommand(Resources.Continue, "Continue", typeof (Page));
        public static RoutedCommand Close = new RoutedUICommand(Resources.Cancel, "Close", typeof (Page));
        public static RoutedCommand ScrollToLetter = new RoutedUICommand(string.Empty, "ScrollToLetter", typeof(Page));
    }
}
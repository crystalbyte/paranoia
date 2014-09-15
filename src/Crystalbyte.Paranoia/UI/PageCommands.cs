#region Using directives

using System.Windows.Controls;
using System.Windows.Input;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI.Pages;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public static class PageCommands {
        public static RoutedCommand Continue = new RoutedUICommand(Resources.Continue, "Continue", typeof (Page));
        public static RoutedCommand Cancel = new RoutedUICommand(Resources.Cancel, "Cancel", typeof (Page));
        public static RoutedCommand ScrollToLetter = new RoutedUICommand(string.Empty, "ScrollToLetter", typeof(Page));
    }
}
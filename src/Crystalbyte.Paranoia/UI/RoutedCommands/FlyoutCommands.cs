#region Using directives

using System.Windows.Input;
using Crystalbyte.Paranoia.Properties;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public static class FlyoutCommands {
        public static RoutedCommand Continue = new RoutedUICommand(Resources.Continue, "Continue", typeof(FlyoutCommands));
        public static RoutedCommand Back = new RoutedUICommand(Resources.Back, "Back", typeof(FlyoutCommands));
        public static RoutedCommand Close = new RoutedUICommand(Resources.Cancel, "Close", typeof(FlyoutCommands));
    }
}
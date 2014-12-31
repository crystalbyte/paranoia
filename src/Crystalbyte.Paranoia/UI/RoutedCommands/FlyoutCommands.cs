#region Using directives

using System.Windows.Input;
using Crystalbyte.Paranoia.Properties;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public static class FlyoutCommands {
        public static RoutedCommand Continue = new RoutedUICommand(Resources.Continue, "Continue", typeof(FlyoutCommands));
        public static RoutedCommand Accept = new RoutedUICommand(Resources.Accept, "Accept", typeof(FlyoutCommands));
        public static RoutedCommand Back = new RoutedUICommand(Resources.Back, "Back", typeof(FlyoutCommands));
        public static RoutedCommand Cancel = new RoutedUICommand(Resources.Cancel, "Cancel", typeof(FlyoutCommands));
    }
}
#region Using directives

using System.Windows.Input;
using Crystalbyte.Paranoia.UI.Properties;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public static class WindowCommands {
        public static RoutedUICommand Minimize =
            new RoutedUICommand(Resources.MinimizeCommand, "Minimize", typeof (MetroWindow));

        public static RoutedUICommand Maximize =
            new RoutedUICommand(Resources.MaximizeCommand, "Maximize", typeof (MetroWindow));

        public static RoutedUICommand RestoreDown =
            new RoutedUICommand(Resources.RestoreDownCommand, "RestoreDown", typeof (MetroWindow));
    }
}
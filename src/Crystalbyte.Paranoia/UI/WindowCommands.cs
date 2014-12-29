#region Using directives

using System.Windows.Input;
using Crystalbyte.Paranoia.Properties;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public static class WindowCommands {
        public static RoutedUICommand Minimize =
            new RoutedUICommand(Resources.Minimize, "Minimize", typeof (MetroWindow));

        public static RoutedUICommand Maximize =
            new RoutedUICommand(Resources.Maximize, "Maximize", typeof (MetroWindow));

        public static RoutedUICommand RestoreDown =
            new RoutedUICommand(Resources.RestoreDown, "RestoreDown", typeof (MetroWindow));
    }
}
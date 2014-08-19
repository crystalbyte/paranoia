#region Using directives

using System.Windows.Input;
using Crystalbyte.Paranoia.Properties;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public static class WindowCommands {
        public static RoutedUICommand Minimize =
            new RoutedUICommand(Resources.MinimizeCommandTooltip, "Minimize", typeof (MetroWindow));

        public static RoutedUICommand Maximize =
            new RoutedUICommand(Resources.MaximizeCommandTooltip, "Maximize", typeof (MetroWindow));

        public static RoutedUICommand RestoreDown =
            new RoutedUICommand(Resources.RestoreDownCommandTooltip, "RestoreDown", typeof (MetroWindow));

        public static RoutedUICommand CloseFlyOut =
            new RoutedUICommand(Resources.CloseFlyOutCommandTooltip, "CloseFlyOut", typeof (MetroWindow));

        public static RoutedCommand OpenAccountMenu =
            new RoutedCommand("OpenAccountMenu", typeof (MetroWindow));
    }
}
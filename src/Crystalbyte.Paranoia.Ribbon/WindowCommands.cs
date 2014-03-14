#region Using directives

using System.Windows.Input;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI;

#endregion

namespace Crystalbyte.Paranoia {
    public static class WindowCommands {
        public static RoutedUICommand Minimize = 
            new RoutedUICommand(Resources.MinimizeCommandTooltip, Resources.MinimizeCommandName, typeof(RibbonWindow));

        public static RoutedUICommand Maximize = 
            new RoutedUICommand(Resources.MaximizeCommandTooltip, Resources.MaximizeCommandName, typeof(RibbonWindow));

        public static RoutedUICommand Close = 
            new RoutedUICommand(Resources.CloseCommandTooltip, Resources.CloseCommandName, typeof(RibbonWindow));

        public static RoutedUICommand RestoreDown = 
            new RoutedUICommand(Resources.RestoreDownCommandTooltip, Resources.RestoreDownCommandName, typeof(RibbonWindow));

        public static RoutedUICommand Back = 
            new RoutedUICommand(Resources.BackCommandTooltip, Resources.BackCommandName, typeof(RibbonWindow));
    }
}
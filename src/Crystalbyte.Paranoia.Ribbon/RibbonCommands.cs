#region Using directives

using System.Windows.Input;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI;

#endregion

namespace Crystalbyte.Paranoia {
    public static class RibbonCommands {
        public static RoutedUICommand OpenAppMenu =
            new RoutedUICommand(Resources.OpenAppMenuCommandTooltip, Resources.OpenAppMenuCommandName, typeof(Ribbon));

        public static RoutedUICommand BlendInRibbon =
            new RoutedUICommand(Resources.BlendInRibbonCommandTooltip, Resources.BlendInRibbonCommandName, typeof(RibbonWindow));

        public static RoutedUICommand OpenRibbonOptions =
            new RoutedUICommand(Resources.OpenRibbonOptionsCommandTooltip, Resources.OpenRibbonOptionsCommandName, typeof(RibbonWindow));

        public static RoutedUICommand AddQuickAccess =
            new RoutedUICommand(Resources.AddQuickAccessCommandTooltip, Resources.AddQuickAccessCommandName, typeof(RibbonWindow));
    }
}
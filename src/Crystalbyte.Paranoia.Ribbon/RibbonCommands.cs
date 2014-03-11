#region Using directives

using System.Windows.Input;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI;

#endregion

namespace Crystalbyte.Paranoia {
    public static class RibbonCommands {
        public static RoutedCommand OpenAppMenu =
            new RoutedUICommand(Resources.OpenAppMenuCommandTooltip, Resources.OpenAppMenuCommandName, typeof(Ribbon));
    }
}
#region Using directives

using System.Windows.Input;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI;

#endregion

namespace Crystalbyte.Paranoia {
    public static class RibbonCommands {
        public static RoutedCommand DisplayAppMenu = 
            new RoutedUICommand(Resources.AppMenuText, "DisplayAppMenu", typeof (Ribbon));
    }
}
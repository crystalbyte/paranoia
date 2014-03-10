#region Using directives

using System.Windows.Input;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI;

#endregion

namespace Crystalbyte.Paranoia {
    public static class WindowCommands {
        public static RoutedUICommand Minimize = 
            new RoutedUICommand(Resources.MinimizeCommandText, "Minimize", typeof(RibbonWindow));

        public static RoutedUICommand Maximize = 
            new RoutedUICommand(Resources.MinimizeCommandText, "Maximize", typeof(RibbonWindow));

        public static RoutedUICommand Close = 
            new RoutedUICommand(Resources.MinimizeCommandText, "Copy", typeof(RibbonWindow));

        public static RoutedUICommand RestoreDown = 
            new RoutedUICommand(Resources.MinimizeCommandText, "RestoreDown", typeof(RibbonWindow));

        public static RoutedUICommand Back = 
            new RoutedUICommand(Resources.AppMenuText, "Back", typeof(RibbonWindow));
    }
}
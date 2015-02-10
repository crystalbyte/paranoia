using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI {
    public static class HtmlCommands {
        public static RoutedCommand ViewSource = new RoutedUICommand(string.Empty, "ViewSource", typeof(HtmlCommands));
    }
}

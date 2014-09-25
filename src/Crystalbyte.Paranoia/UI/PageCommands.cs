#region Using directives

using System.Windows.Controls;
using System.Windows.Input;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI.Pages;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public static class PageCommands {
        public static RoutedCommand Continue = new RoutedUICommand(Resources.Continue, "Continue", typeof (Page));
        public static RoutedCommand Cancel = new RoutedUICommand(Resources.Cancel, "Cancel", typeof (Page));
        public static RoutedCommand ScrollToLetter = new RoutedUICommand(string.Empty, "ScrollToLetter", typeof(Page));
        public static RoutedCommand SelectMailboxRole = new RoutedUICommand(string.Empty, "SelectMailboxRole", typeof(Page));
        public static RoutedCommand CancelMailboxRoleSelection = new RoutedUICommand(string.Empty, "CancelMailboxRoleSelection", typeof(Page));
        public static RoutedCommand CommitMailboxRoleSelection = new RoutedUICommand(string.Empty, "CommitMailboxRoleSelection", typeof(Page));

    }
}
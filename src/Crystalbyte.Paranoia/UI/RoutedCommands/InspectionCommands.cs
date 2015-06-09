using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI {
    public static class InspectionCommands {
        public static RoutedCommand ShowCcAddresses = new RoutedCommand("ShowCcAddresses", typeof(InspectionCommands));
        public static RoutedCommand ShowToAddresses = new RoutedCommand("ShowToAddresses", typeof(InspectionCommands));
    }
}

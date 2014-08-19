#region Using directives

using System.Windows.Navigation;

#endregion

namespace Crystalbyte.Paranoia.UI {
    internal interface INavigationAware {
        void OnNavigated(NavigationEventArgs e);
    }
}
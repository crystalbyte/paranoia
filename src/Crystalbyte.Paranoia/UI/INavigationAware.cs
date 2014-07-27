using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;


namespace Crystalbyte.Paranoia.UI {
    internal interface INavigationAware {
        void OnNavigated(NavigationEventArgs e);
    }
}

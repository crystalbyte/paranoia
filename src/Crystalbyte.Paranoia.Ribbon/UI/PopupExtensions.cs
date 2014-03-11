using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace Crystalbyte.Paranoia.UI {
    public static class PopupExtensions {
        public static void FlyInFromTop(this Popup popup, RibbonWindow window) {
            popup.PlacementTarget = window;
            popup.Placement = PlacementMode.Top;
            popup.StaysOpen = false;
            popup.IsOpen = true;
        }
    }
}

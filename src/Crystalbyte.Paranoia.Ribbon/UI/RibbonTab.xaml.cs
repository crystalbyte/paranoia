#region Using directives

using System.Windows;
using System.Windows.Controls;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public class RibbonTab : TabItem {

        #region Construction

        static RibbonTab() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonTab),
                new FrameworkPropertyMetadata(typeof(RibbonTab)));
        }

        #endregion
    }
}
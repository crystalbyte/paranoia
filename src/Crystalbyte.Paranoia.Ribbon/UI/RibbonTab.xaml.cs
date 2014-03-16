#region Using directives

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public class RibbonTab : TabItem {

        #region Construction

        static RibbonTab() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonTab),
                new FrameworkPropertyMetadata(typeof(RibbonTab)));
        }

        #endregion

        #region Class Overrides

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonUp(e);

            // Unfortunately the TabControl does not allow for tab deselections without selecting a new one.
            // Since the deselect tab remains selected internally it won't be automatically reselected on mouse up.
            // We need to set it manually.
            IsSelected = true;
        }

        #endregion
    }
}
#region Using directives

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

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
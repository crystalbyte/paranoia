#region Using directives

using System.Windows;
using System.Windows.Controls;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public class Ribbon : TabControl {

        #region Construction

        static Ribbon() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Ribbon), 
                new FrameworkPropertyMetadata(typeof(Ribbon)));
        }

        #endregion

        #region Class Overrides

        protected override DependencyObject GetContainerForItemOverride() {
            return new RibbonTab();
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is RibbonTab;
        }

        #endregion

        #region Dependency Properties

        public string AppMenuText {
            get { return (string)GetValue(AppMenuTextProperty); }
            set { SetValue(AppMenuTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AppMenuButtonContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AppMenuTextProperty =
            DependencyProperty.Register("AppMenuText", typeof(string), typeof(Ribbon),
                new PropertyMetadata(string.Empty));

        #endregion
    }
}
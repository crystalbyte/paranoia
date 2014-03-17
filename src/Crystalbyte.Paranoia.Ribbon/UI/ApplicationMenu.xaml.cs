using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI {
    public class ApplicationMenu : TabControl {

        #region Construction

        static ApplicationMenu() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ApplicationMenu),
                new FrameworkPropertyMetadata(typeof(ApplicationMenu)));
        }

        #endregion

        #region Class Overrides

        protected override void OnSelectionChanged(SelectionChangedEventArgs e) {
            base.OnSelectionChanged(e);

            if (SelectedIndex == -1) {
                SelectedHeader = null;
            }

            var item = ItemContainerGenerator.ContainerFromIndex(SelectedIndex) as ApplicationMenuItem;
            if (item != null) {
                SelectedHeader = item.Header;
            }
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return new ApplicationMenuItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is ApplicationMenuItem
                   || item is ApplicationMenuSeparator
                   || item is ApplicationMenuButton;
        }

        #endregion

        #region Dependency Properties

        public object SelectedHeader {
            get { return GetValue(SelectedHeaderProperty); }
            private set { SetValue(SelectedHeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedHeader.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedHeaderProperty =
            DependencyProperty.Register("SelectedHeader", typeof(object), typeof(ApplicationMenu), new PropertyMetadata(null));

        #endregion
    }
}

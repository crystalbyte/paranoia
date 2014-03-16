using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI {
    public sealed class ApplicationMenu : TabControl {

        #region Construction

        static ApplicationMenu() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ApplicationMenu),
                new FrameworkPropertyMetadata(typeof(ApplicationMenu)));
        }

        #endregion

        #region Class Overrides

        protected override DependencyObject GetContainerForItemOverride() {
            return new ApplicationMenuItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is ApplicationMenuItem;
        }

        #endregion
    }
}

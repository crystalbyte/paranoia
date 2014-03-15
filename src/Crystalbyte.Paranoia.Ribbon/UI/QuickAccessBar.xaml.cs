using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for QuickAccessBar.xaml
    /// </summary>
    public class QuickAccessBar : ItemsControl {

        #region Construction

        static QuickAccessBar() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(QuickAccessBar),
             new FrameworkPropertyMetadata(typeof(QuickAccessBar)));
        }

        #endregion

        #region Class Overrides

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is QuickAccessBarItem;
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return new QuickAccessBarItem();
        }

        #endregion
    }
}

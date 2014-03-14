using System.Windows;
using System.Windows.Controls;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for RibbonPage.xaml
    /// </summary>
    public class RibbonPage : ItemsControl {
        static RibbonPage() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonPage),
              new FrameworkPropertyMetadata(typeof(RibbonPage)));
        }
    }
}

using System.Windows;

namespace Crystalbyte.Paranoia.UI {
    public static class Pin {

        #region Attached Properties

        public static UIElement GetTargetElement(DependencyObject d) {
            return (UIElement)d.GetValue(TargetElementProperty); 
        }

        public static void SetTargetElement(DependencyObject d, UIElement element) {
            d.SetValue(TargetElementProperty, element);
        }

        // Using a DependencyProperty as the backing store for TargetElement.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetElementProperty =
            DependencyProperty.RegisterAttached("TargetElement", typeof(UIElement), typeof(Pin), new PropertyMetadata(null));

        #endregion
    }
}

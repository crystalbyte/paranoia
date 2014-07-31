using System.Windows;
using System.Windows.Controls;

namespace Crystalbyte.Paranoia.UI {
    public sealed class MetroCircleButton : Button {

        #region Construction

        static MetroCircleButton() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MetroCircleButton),
                new FrameworkPropertyMetadata(typeof(MetroCircleButton)));
        }

        #endregion

        #region Dependency Properties

        public string Text {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MetroCircleButton), new PropertyMetadata(null));

        #endregion
    }
}

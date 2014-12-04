#region Using directives

using System.Windows;
using System.Windows.Controls;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public sealed class MetroButton : Button {

        #region Construction

        static MetroButton() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (MetroButton),
                new FrameworkPropertyMetadata(typeof (MetroButton)));
        }

        #endregion

        #region Dependency Properties

        public string Text {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof (string), typeof (MetroButton), new PropertyMetadata(null));

        #endregion
    }
}
#region Using directives

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        public ImageSource ImageSource {
            get { return (ImageSource) GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(MetroButton), new PropertyMetadata(null));

        public Brush AccentBrush {
            get { return (Brush)GetValue(AccentBrushProperty); }
            set { SetValue(AccentBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AccentBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AccentBrushProperty =
            DependencyProperty.Register("AccentBrush", typeof(Brush), typeof(MetroButton), new PropertyMetadata(null));

        public Brush ImageBrush {
            get { return (Brush)GetValue(ImageBrushProperty); }
            set { SetValue(ImageBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageBrushProperty =
            DependencyProperty.Register("ImageBrush", typeof(Brush), typeof(MetroButton), new PropertyMetadata(null));

        #endregion
    }
}
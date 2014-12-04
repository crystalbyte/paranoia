using System.Windows;
using System.Windows.Controls;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for WatermarkTextBox.xaml
    /// </summary>
    public class WatermarkTextBox : TextBox {

        #region Construction

        static WatermarkTextBox() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WatermarkTextBox),
                new FrameworkPropertyMetadata(typeof(WatermarkTextBox)));
        }

        #endregion

        #region Dependency Properties

        public object Watermark {
            get { return GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark", typeof(object), typeof(WatermarkTextBox), new PropertyMetadata(null));


        public DataTemplate WatermarkTemplate {
            get { return (DataTemplate)GetValue(WatermarkTemplateProperty); }
            set { SetValue(WatermarkTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WatermarkTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WatermarkTemplateProperty =
            DependencyProperty.Register("WatermarkTemplate", typeof(DataTemplate), typeof(WatermarkTextBox), new PropertyMetadata(null));

        #endregion
    }
}

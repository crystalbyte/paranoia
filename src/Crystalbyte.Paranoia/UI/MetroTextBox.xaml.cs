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
    /// Interaction logic for MetroTextBox.xaml
    /// </summary>
    public class MetroTextBox : TextBox {

        #region Construction

        static MetroTextBox() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MetroTextBox),
                new FrameworkPropertyMetadata(typeof(MetroTextBox)));
        }

        #endregion

        #region Dependency Properties

        public object Watermark {
            get { return GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark", typeof(object), typeof(MetroTextBox), new PropertyMetadata(null));


        public DataTemplate WatermarkTemplate {
            get { return (DataTemplate)GetValue(WatermarkTemplateProperty); }
            set { SetValue(WatermarkTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WatermarkTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WatermarkTemplateProperty =
            DependencyProperty.Register("WatermarkTemplate", typeof(DataTemplate), typeof(MetroTextBox), new PropertyMetadata(null));

        #endregion
    }
}

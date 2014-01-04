using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Crystalbyte.Paranoia.Converters {

    [ValueConversion(typeof(bool), typeof(Brush))]
    public sealed class BooleanToBrushConverter : IValueConverter {

        public Brush PositiveBrush { get; set; }
        public Brush NegativeBrush { get; set; }

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (bool) value ? PositiveBrush : NegativeBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }

        #endregion
    }
}

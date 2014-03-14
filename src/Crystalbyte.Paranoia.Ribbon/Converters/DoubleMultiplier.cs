using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Crystalbyte.Paranoia.Converters {
    [ValueConversion(typeof(double), typeof(double))]
    public sealed class DoubleMultiplier : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var multiplier = double.Parse((string)parameter);
            var basis = (double) value;
            if (double.IsNaN(basis)) {
                return value;
            }
            return basis*multiplier;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Crystalbyte.Paranoia.Converters {
    [ValueConversion(typeof(double), typeof(double))]
    public sealed class RatioConverter : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var t = (double)value;
            var p = double.Parse((string)parameter);
            return t / p;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }

        #endregion
    }
}

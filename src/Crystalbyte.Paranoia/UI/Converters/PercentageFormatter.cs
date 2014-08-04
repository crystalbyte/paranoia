using System;
using System.Globalization;
using System.Windows.Data;

namespace Crystalbyte.Paranoia.UI.Converters {
    [ValueConversion(typeof(double), typeof(string))]
    public sealed class PercentageFormatter : IValueConverter {

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var i = (float)value;
            return string.Format("{0:F0}%", i);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }

        #endregion
    }
}

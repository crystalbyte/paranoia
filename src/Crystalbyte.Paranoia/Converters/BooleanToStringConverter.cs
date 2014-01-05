using System;
using System.Globalization;
using System.Windows.Data;

namespace Crystalbyte.Paranoia.Converters {
    [ValueConversion(typeof(bool), typeof(string))]
    public sealed class BooleanToStringConverter : IValueConverter {

        public string PositiveText { get; set; }
        public string NegativeText { get; set; }

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((bool) value) ? PositiveText : NegativeText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }

        #endregion
    }
}

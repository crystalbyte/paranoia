using System;
using System.Globalization;
using System.Windows.Data;

namespace Crystalbyte.Paranoia.UI.Converters {
    [ValueConversion(typeof(bool), typeof(double))]
    public sealed class BooleanToOpacityConverter : IValueConverter {

        public double OpacityForTrue { get; set; }
        public double OpacityForFalse { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((bool) value) ? OpacityForTrue : OpacityForFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}

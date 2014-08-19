#region Using directives

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#endregion

namespace Crystalbyte.Paranoia.UI.Converters {
    [ValueConversion(typeof (int), typeof (Visibility))]
    public sealed class QuantityToVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var p = parameter as string;
            if (string.IsNullOrEmpty(p) && p == "!") {
                return ((int) value) > 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            return ((int) value) > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}
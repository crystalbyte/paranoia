#region Using directives

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#endregion

namespace Crystalbyte.Paranoia.Converters {
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class BooleanToVisibilityConverter : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var text = parameter as string;
            if (string.IsNullOrWhiteSpace(text)) {
                return ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
            }

            var hide = text.Contains("h");

            if (text.Contains("!")) {
                return ((bool)value)
                    ? hide
                        ? Visibility.Hidden
                        : Visibility.Collapsed
                    : Visibility.Visible;
            }

            return ((bool)value)
                ? Visibility.Visible
                : hide
                        ? Visibility.Hidden
                        : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }

        #endregion
    }
}
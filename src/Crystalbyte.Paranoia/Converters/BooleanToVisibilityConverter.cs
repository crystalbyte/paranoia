#region Using directives

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#endregion

namespace Crystalbyte.Paranoia.Converters {
    [ValueConversion(typeof (bool), typeof (Visibility))]
    public sealed class BooleanToVisibilityConverter : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (string.IsNullOrWhiteSpace(parameter as string)) {
                return ((bool) value) ? Visibility.Visible : Visibility.Collapsed;
            }

            return ((bool) value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
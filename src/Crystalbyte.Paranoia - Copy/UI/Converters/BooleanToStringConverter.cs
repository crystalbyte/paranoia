#region Using directives

using System;
using System.Globalization;
using System.Windows.Data;

#endregion

namespace Crystalbyte.Paranoia.UI.Converters {
    public sealed class BooleanToStringConverter : IValueConverter {
        public string StringForTrue { get; set; }
        public string StringForFalse { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((bool) value) ? StringForTrue : StringForFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}
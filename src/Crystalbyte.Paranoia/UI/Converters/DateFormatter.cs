using System;
using System.Globalization;
using System.Windows.Data;

namespace Crystalbyte.Paranoia.UI.Converters {
    [ValueConversion(typeof(DateTime), typeof(string))]
    public sealed class DateFormatter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var current = DateTime.Now;
            var date = (DateTime)value;
            if (current.DayOfYear == date.DayOfYear && current.Year == date.Year) {
                return date.TimeOfDay;
            }

            return date.ToShortDateString();

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}

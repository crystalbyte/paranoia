using Crystalbyte.Paranoia.Properties;
using System;
using System.Windows.Data;

namespace Crystalbyte.Paranoia.Converters {
    [ValueConversion(typeof(DateTime), typeof(string))]
    public sealed class DateFormatter : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek));
            var date = (DateTime)value;

            if (today == date.Date) {
                return Resources.TodayText;
            }

            if (date.Date.AddDays(1) == today) {
                return Resources.YesterdayText;
            }

            return date.Date > startOfWeek.Date ? date.DayOfWeek.ToString() : date.ToShortDateString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return Binding.DoNothing;
        }

        #endregion
    }
}

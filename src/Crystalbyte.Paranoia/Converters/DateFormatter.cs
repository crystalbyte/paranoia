using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Crystalbyte.Paranoia.Converters {
    public sealed class DateFormatter : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek));
            var date = (DateTime)value;

            if (date.Date > startOfWeek.Date && date.Date < today) {
                return date.DayOfWeek.ToString();
            }

            return date.ToLocalTime();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}

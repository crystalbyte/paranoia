using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Crystalbyte.Paranoia.Converters {
    [ValueConversion(typeof(long), typeof(string))]
    public sealed class SizeFormatter : IValueConverter {

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var size = (long)value;
            if (size < 1000) {
                return string.Format("{0} B", size);
            }
            if (size >= 1000 && size < 1000000) {
                return string.Format("{0} kB", size / 1000);
            }

            return string.Format("{0} MB", size / 1000000);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return Binding.DoNothing;
        }

        #endregion
    }
}

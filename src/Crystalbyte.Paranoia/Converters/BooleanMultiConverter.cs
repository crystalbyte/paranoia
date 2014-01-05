using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Crystalbyte.Paranoia.Converters {
    [ValueConversion(typeof(bool[]), typeof(bool))]
    public sealed class BooleanMultiConverter : IMultiValueConverter {
        #region Implementation of IMultiValueConverter

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            var op = parameter as string;
            return op == "AND" 
                ? values.OfType<bool>().All(x => x) 
                : values.OfType<bool>().Any(x => x);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            return new object[0];
        }

        #endregion
    }
}

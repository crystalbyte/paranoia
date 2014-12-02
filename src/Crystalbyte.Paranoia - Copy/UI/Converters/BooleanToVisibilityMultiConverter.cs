using System;
using System.Linq;
using System.Windows.Data;

namespace Crystalbyte.Paranoia.UI.Converters {
    public sealed class BooleanToVisibilityMultiConverter : IMultiValueConverter {

        #region IMultiValueConverter

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var booleans = values.Select(x => (bool) x);

            var s = parameter as string;
            if (string.IsNullOrEmpty(s) || string.Compare(s, "AND", StringComparison.InvariantCultureIgnoreCase) == 0) {
                return booleans.All(x => x);
            }

            return booleans.Any(x => x);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) {
            return new[] { Binding.DoNothing };
        }

        #endregion
    }
}

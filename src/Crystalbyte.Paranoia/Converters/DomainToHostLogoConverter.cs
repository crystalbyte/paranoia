using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Crystalbyte.Paranoia.Converters {
    [ValueConversion(typeof(string), typeof(string))]
    public sealed class DomainToHostLogoConverter : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var address = value as string;
            if (string.IsNullOrWhiteSpace(address)) {
                // TODO: default
                return string.Empty;
            }

            if (address.Contains("gmail") || address.Contains("googlemail")) {
                return "/Assets/googlemail.png";
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }

        #endregion
    }
}

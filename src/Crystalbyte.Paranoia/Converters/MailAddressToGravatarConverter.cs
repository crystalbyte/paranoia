using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Crystalbyte.Paranoia.Converters {
    [ValueConversion(typeof(string), typeof(string))]
    public sealed class MailAddressToGravatarConverter : IValueConverter {

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return Gravatar.CreateImageUrl(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return Binding.DoNothing;
        }

        #endregion
    }
}

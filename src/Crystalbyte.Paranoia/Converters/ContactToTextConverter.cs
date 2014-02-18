using Crystalbyte.Paranoia.Contexts;
using Crystalbyte.Paranoia.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Crystalbyte.Paranoia.Converters {
    [ValueConversion(typeof(ContactContext), typeof(string))]
    public sealed class ContactToTextConverter : IValueConverter {

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var contact = value as ContactContext;
            if (contact == null) {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(contact.Name)) {
                return Resources.RequestPendingMessage;
            }

            return contact.Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return Binding.DoNothing;
        }

        #endregion
    }
}

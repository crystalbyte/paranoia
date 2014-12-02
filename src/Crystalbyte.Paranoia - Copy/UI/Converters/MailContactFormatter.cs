#region Using directives

using System;
using System.Globalization;
using System.Windows.Data;

#endregion

namespace Crystalbyte.Paranoia.UI.Converters {
    [ValueConversion(typeof (string), typeof (string))]
    public sealed class MailContactFormatter : IValueConverter {

        private static readonly NullOrEmptyFormatter NullOrEmptyFormatter = new NullOrEmptyFormatter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var s =  value as MailMessageContext;
            if (s == null) {
                return NullOrEmptyFormatter.Convert(null, targetType, parameter, culture);
            }

            if (s.FromName.Equals("NIL") || string.IsNullOrEmpty(s.FromName)) {
                return NullOrEmptyFormatter.Convert(s.FromAddress, targetType, parameter, culture);
            }
            return NullOrEmptyFormatter.Convert(s.FromName, targetType, parameter, culture);
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}
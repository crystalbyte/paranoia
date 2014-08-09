#region Using directives

using System;
using System.Globalization;
using System.Windows.Data;

#endregion

namespace Crystalbyte.Paranoia.UI.Converters {
    [ValueConversion(typeof(string), typeof(string))]
    public sealed class FromSenderFormatter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var s = (MailMessageContext)value;

            if (s.FromName.Equals("NIL") || string.IsNullOrEmpty(s.FromName)) {
                return s.FromAddress;
            }
            return s.FromName;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}
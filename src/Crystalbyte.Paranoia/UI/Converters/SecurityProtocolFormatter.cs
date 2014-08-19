#region Using directives

using System;
using System.Globalization;
using System.Windows.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;

#endregion

namespace Crystalbyte.Paranoia.UI.Converters {
    [ValueConversion(typeof (SecurityProtocol), typeof (string))]
    public sealed class SecurityProtocolFormatter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((SecurityProtocol) value) == SecurityProtocol.Implicit ? Resources.Implicit : Resources.Explicit;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}
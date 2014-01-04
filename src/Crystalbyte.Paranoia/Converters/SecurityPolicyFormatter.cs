#region Using directives

using System;
using System.Globalization;
using System.Windows.Data;
using Crystalbyte.Paranoia.Messaging;

#endregion

namespace Crystalbyte.Paranoia.Converters {
    [ValueConversion(typeof (SecurityPolicy), typeof (string))]
    public sealed class SecurityPolicyFormatter : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var v = (SecurityPolicy) value;
            switch (v) {
                case SecurityPolicy.None:
                    return "None";
                case SecurityPolicy.Implicit:
                    return "Implicit (SSL)";
                case SecurityPolicy.Explicit:
                    return "Explicit (TLS)";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }

        #endregion
    }
}
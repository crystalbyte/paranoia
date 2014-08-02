#region Using directives

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

#endregion

namespace Crystalbyte.Paranoia.UI.Converters {
    [ValueConversion(typeof (string), typeof (string))]
    public sealed class SubjectFormater : IValueConverter {
        private const string Sub = " \u200B";
        private const string Pattern = "\n|\r";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var s = (string) value;

            if (string.IsNullOrEmpty(s))
            {
                return s;
            }
            if (s.Equals("NIL"))
            {
                return s = Properties.Resources.NoSubject;
            }
           else
            {
                return Regex.Replace(s, Pattern, Sub);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var s = (string) value;
            return string.IsNullOrEmpty(s) ? s : Regex.Replace(s, Sub, Pattern);
        }
    }
}
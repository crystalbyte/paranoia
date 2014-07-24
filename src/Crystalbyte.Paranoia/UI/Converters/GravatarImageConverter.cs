#region Using directives

using System;
using System.Globalization;
using System.Windows.Data;

#endregion

namespace Crystalbyte.Paranoia.UI.Converters {
    [ValueConversion(typeof (string), typeof (string))]
    public sealed class GravatarImageConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var p = parameter as string;
            return !string.IsNullOrEmpty(p) 
                ? Gravatar.CreateImageUrl((string)value, int.Parse(p)) 
                : Gravatar.CreateImageUrl((string) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}
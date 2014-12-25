using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace Crystalbyte.Paranoia.UI.Converters {
    public sealed class SortPropertyFormatter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var property = (SortProperty)value;

            var attributes =
                typeof(SortProperty).GetField(property.ToString())
                    .GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Length <= 0) 
                return property;

            var description = (DescriptionAttribute)attributes[0];
            return description.Type.GetProperty(description.Name)
                .GetValue(null, BindingFlags.Static, null, null, CultureInfo.CurrentCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}

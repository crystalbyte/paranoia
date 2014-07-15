#region Using directives

using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

#endregion

namespace Crystalbyte.Paranoia.UI.Converters {
    [ValueConversion(typeof (ICollection), typeof (int))]
    public sealed class CollectionCounter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var collection = value as ICollection;
            return collection == null ? 0 : collection.Count;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}
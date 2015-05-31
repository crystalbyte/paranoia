using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Crystalbyte.Paranoia.UI.Converters {
    public sealed class CollectionIndexer : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            // TODO: Performance might be improved by using collection casts and using the class indexer instead of IEnumerable.
            var collection = value as IEnumerable;
            var p = parameter as string;
            var index = string.IsNullOrEmpty(p) ? 0 : System.Convert.ToInt32(p);
            return collection == null ? value : collection.OfType<object>().ElementAt(index);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace Crystalbyte.Paranoia.Converters {
    
    [ContentProperty("Converters")]
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class ConversionGroup : IValueConverter {

        #region Construction

        public ConversionGroup() {
            Converters = new List<IValueConverter>();
        }

        #endregion

        public List<IValueConverter> Converters { get; set; }

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return Converters.Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }

        #endregion
    }
}

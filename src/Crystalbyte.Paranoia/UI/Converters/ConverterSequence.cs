using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Crystalbyte.Paranoia.UI.Converters {
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class ConverterSequence : IValueConverter {

        #region Properties

        public List<IValueConverter> Converters { get; set; }

        #endregion

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

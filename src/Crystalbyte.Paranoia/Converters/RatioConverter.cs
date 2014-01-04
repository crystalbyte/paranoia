﻿#region Using directives

using System;
using System.Globalization;
using System.Windows.Data;

#endregion

namespace Crystalbyte.Paranoia.Converters {
    [ValueConversion(typeof (double), typeof (double))]
    public sealed class RatioConverter : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var t = (double) value;
            var p = double.Parse((string) parameter);
            return t/p;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }

        #endregion
    }
}
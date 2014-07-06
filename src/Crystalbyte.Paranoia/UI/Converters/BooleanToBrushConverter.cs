using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Crystalbyte.Paranoia.UI.Converters {
    [ValueConversion(typeof(bool), typeof(Brush))]
    public sealed class BooleanToBrushConverter : IValueConverter {

        public Brush BrushForTrue { get; set; }
        public Brush BrushForFalse { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((bool)value) ? BrushForTrue : BrushForFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}

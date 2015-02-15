using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using dotless.Core.Parser.Tree;
using Color = System.Windows.Media.Color;

namespace Crystalbyte.Paranoia.UI.Converters {
    [ValueConversion(typeof(Color), typeof(SolidColorBrush))]
    public sealed class ColorToBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return new SolidColorBrush((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}

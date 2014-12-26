using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Effects;

namespace Crystalbyte.Paranoia.UI.Converters {
    public sealed class BooleanToBlurEffectConverter : IValueConverter {

        private readonly BlurEffect _blurEffect = new BlurEffect {
            Radius = 10, RenderingBias = RenderingBias.Performance
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var boolean = (bool)value;
            return boolean ? _blurEffect : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}

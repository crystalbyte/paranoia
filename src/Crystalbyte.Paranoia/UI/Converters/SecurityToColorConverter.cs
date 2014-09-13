using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Crystalbyte.Paranoia.UI.Converters {

    [ValueConversion(typeof(SecurityMeasure), typeof(Brush))]
    public sealed class SecurityToColorConverter : IValueConverter {

        private readonly static Brush RedBrush = new SolidColorBrush(Colors.Red);
        private readonly static Brush YellowBrush = new SolidColorBrush(Colors.Yellow);
        private readonly static Brush GreenBrush = new SolidColorBrush(Colors.Green);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var sec = (SecurityMeasure)value;
            switch (sec) {
                case SecurityMeasure.None:
                    return RedBrush;
                case SecurityMeasure.Encrypted:
                    return YellowBrush;
                case SecurityMeasure.EncryptedAndVerified:
                    return GreenBrush;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}

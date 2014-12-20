#region Using directives

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#endregion

namespace Crystalbyte.Paranoia.UI.Converters {
    [ValueConversion(typeof(bool), typeof(double))]
    public sealed class BooleanToOpacityConverter : DependencyObject, IValueConverter {

        public double OpacityForFalse {
            get { return (double)GetValue(OpacityForFalseProperty); }
            set { SetValue(OpacityForFalseProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OpacityForFalse.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpacityForFalseProperty =
            DependencyProperty.Register("OpacityForFalse", typeof(double), typeof(BooleanToOpacityConverter), new PropertyMetadata(1.0d));

        public double OpacityForTrue {
            get { return (double)GetValue(OpacityForTrueProperty); }
            set { SetValue(OpacityForTrueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OpacityForTrue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpacityForTrueProperty =
            DependencyProperty.Register("OpacityForTrue", typeof(double), typeof(BooleanToOpacityConverter), new PropertyMetadata(1.0d));


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((bool)value) ? OpacityForTrue : OpacityForFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}
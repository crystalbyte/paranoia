#region Using directives

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Brush = System.Windows.Media.Brush;

#endregion

namespace Crystalbyte.Paranoia.UI.Converters {
    [ValueConversion(typeof (bool), typeof (Brush))]
    public sealed class BooleanToBrushConverter : DependencyObject, IValueConverter {

        public Brush BrushForTrue {
            get { return (Brush)GetValue(BrushForTrueProperty); }
            set { SetValue(BrushForTrueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BrushForTrue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BrushForTrueProperty =
            DependencyProperty.Register("BrushForTrue", typeof(Brush), typeof(BooleanToBrushConverter), new PropertyMetadata(null));

        public Brush BrushForFalse {
            get { return (Brush)GetValue(BrushForFalseProperty); }
            set { SetValue(BrushForFalseProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BrushForFalse.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BrushForFalseProperty =
            DependencyProperty.Register("BrushForFalse", typeof(Brush), typeof(BooleanToBrushConverter), new PropertyMetadata(null));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((bool) value) ? BrushForTrue : BrushForFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}
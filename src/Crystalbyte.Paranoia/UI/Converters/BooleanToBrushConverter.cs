#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia
// 
// Crystalbyte.Paranoia is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License.
// 
// Foobar is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Using Directives

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

#endregion

namespace Crystalbyte.Paranoia.UI.Converters {
    [ValueConversion(typeof (bool), typeof (Brush))]
    public sealed class BooleanToBrushConverter : DependencyObject, IValueConverter {
        public Brush BrushForTrue {
            get { return (Brush) GetValue(BrushForTrueProperty); }
            set { SetValue(BrushForTrueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BrushForTrue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BrushForTrueProperty =
            DependencyProperty.Register("BrushForTrue", typeof (Brush), typeof (BooleanToBrushConverter),
                new PropertyMetadata(null));

        public Brush BrushForFalse {
            get { return (Brush) GetValue(BrushForFalseProperty); }
            set { SetValue(BrushForFalseProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BrushForFalse.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BrushForFalseProperty =
            DependencyProperty.Register("BrushForFalse", typeof (Brush), typeof (BooleanToBrushConverter),
                new PropertyMetadata(null));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((bool) value) ? BrushForTrue : BrushForFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}
﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Crystalbyte.Paranoia.UI.Converters {
    public sealed class GravatarImageConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return Gravatar.CreateImageUrl((string) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}

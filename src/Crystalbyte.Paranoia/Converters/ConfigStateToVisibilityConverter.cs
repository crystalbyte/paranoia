using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Crystalbyte.Paranoia.Converters {
    public sealed class ConfigStateToVisibilityConverter : IValueConverter {
        #region Implementation of IValueConverter
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var strings = parameter as string;
            if (string.IsNullOrEmpty(strings)) {
                return Visibility.Collapsed;
            }

            var validEnums = strings.Split('|')
                .Select(x => (ConfigState)Enum.Parse(typeof(ConfigState), x, true)).ToArray();

            return validEnums.Contains((ConfigState)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return Binding.DoNothing;
        }
        #endregion
    }
}

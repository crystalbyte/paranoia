using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace Crystalbyte.Paranoia.Converters {
    public class ColorToStringConverter : IValueConverter{
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var resourceKey = value.GetType().GetProperties().First().GetValue(value);
            var key = new System.Windows.ComponentResourceKey(typeof(Crystalbyte.Paranoia.UI.RibbonWindow), "WindowAccentBrush");
            //var resourceValue = System.Windows.Application.Current.MainWindow.FindResource(key);
            return resourceKey;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}

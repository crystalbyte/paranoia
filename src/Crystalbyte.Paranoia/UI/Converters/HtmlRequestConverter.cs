using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Crystalbyte.Paranoia.UI.Converters {
    [ValueConversion(typeof(string), typeof(Uri))]
    public sealed class HtmlRequestConverter : IValueConverter {

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var html = value as string;
            if (html == null) {
                return new Uri("asset://paranoia/");
            }
            var guid = Guid.NewGuid();
            HtmlStorage.Push(guid, html);

            var url = string.Format("asset://paranoia/{0}", guid);
            return new Uri(url);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }

        #endregion
    }
}

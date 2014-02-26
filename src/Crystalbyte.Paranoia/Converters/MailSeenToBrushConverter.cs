using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Crystalbyte.Paranoia.Contexts;
using System.Windows.Media;

namespace Crystalbyte.Paranoia.Converters {
    [ValueConversion(typeof(MailContext), typeof(Brush))]
    public sealed class MailSeenToBrushConverter : IMultiValueConverter {
        
        public Brush SelectedBrush { get; set; }
        public Brush SeenBrush { get; set; }
        public Brush NotSeenBrush { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var seen = (bool)values[0];
            var selected = (bool)values[1];

            if (selected) {
                return SelectedBrush;
            }

            return seen ? SeenBrush : NotSeenBrush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) {
            return null;
        }
    }
}

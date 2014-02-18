using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Crystalbyte.Paranoia.Converters {
    [ValueConversion(typeof(ContactRequest),typeof(Brush))]
    public sealed class ContactRequestToBrushConverter : IValueConverter {

        public Brush PendingBrush { get; set; }
        public Brush AcceptedBrush { get; set; }

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var config = (ContactRequest)value;
            if (config == ContactRequest.Pending) {
                return PendingBrush;
            }

            return AcceptedBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return Binding.DoNothing;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Crystalbyte.Paranoia.Converters {
    [ValueConversion(typeof(string),typeof(ImageSource))]
    public sealed class CroppedBitmapConverter : IValueConverter {

        private static readonly BitmapImage Empty = new BitmapImage();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            BitmapImage bitmap = null;
            if (value is string) {
                var info = Application.GetResourceStream(new Uri(value as string, UriKind.Relative));
                if (info == null) {
                    return null;
                }

                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = info.Stream;
                bitmap.EndInit();
            }

            if (value is BitmapImage) {
                bitmap = value as BitmapImage;
            }

            if (bitmap == null) {
                return Empty;
            }

            var fcb = new FormatConvertedBitmap();
            fcb.BeginInit();
            fcb.Source = bitmap;
            fcb.EndInit();
            return fcb;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}

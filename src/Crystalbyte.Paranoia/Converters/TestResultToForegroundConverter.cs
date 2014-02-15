using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Crystalbyte.Paranoia.Converters {
    public sealed class TestResultToForegroundConverter : IValueConverter {
        #region Implementation of IValueConverter
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var e = (TestResult)value;
            switch (e) {
                case TestResult.Success:
                    return App.Current.Resources["AppSuccessColorBrush"];
                case TestResult.Failure:
                    return App.Current.Resources["AppFailureColorBrush"];
            }

            return App.Current.Resources["AppLightTextBrush"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return Binding.DoNothing;
        }
        #endregion
    }
}

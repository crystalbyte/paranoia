using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Media;

namespace Crystalbyte.Paranoia {
    public static class MetroColors {

        public static bool TryGetColorByName(string propertyName, out string color) {
            try {

                var value = typeof(MetroColors).GetProperty(propertyName)
                    .GetValue(null, BindingFlags.Static, null, null, CultureInfo.CurrentCulture);

                if (value is Color) {
                    color = ((Color)value).ToString();
                } else {
                    color = string.Empty;
                    return false;
                }

                return true;
            } catch (Exception) {
                color = "fuchsia";
                return false;
            }
        }

        public static object AppAccentColor {
            get { return ColorConverter.ConvertFromString("#0065B2"); }
        }

        public static object AppFailureColor {
            get { return ColorConverter.ConvertFromString("Brown"); }
        }

        public static object AppSuccessColor {
            get { return ColorConverter.ConvertFromString("ForestGreen"); }
        }

        public static object AppWarningColor {
            get { return ColorConverter.ConvertFromString("Orange"); }
        }

        public static object WindowBackgroundColor {
            get { return ColorConverter.ConvertFromString("#0B0B0C"); }
        }

        public static object WindowShadowColor {
            get { return ColorConverter.ConvertFromString("Black"); }
        }

        public static object FlyoutBackgroundColor {
            get { return ColorConverter.ConvertFromString("#252527"); }
        }

        public static object ControlTextColor {
            get { return ColorConverter.ConvertFromString("WhiteSmoke"); }
        }

        public static object ControlBackgroundColor {
            get { return ColorConverter.ConvertFromString("#222224"); }
        }

        public static object ControlSecondaryBackgroundColor {
            get { return ColorConverter.ConvertFromString("#121213"); }
        }

        public static object ControlBorderColor {
            get { return ColorConverter.ConvertFromString("#525252"); }
        }

        public static object ControlHoverColor {
            get { return ColorConverter.ConvertFromString("#333334"); }
        }

        public static object ControlSecondaryTextColor {
            get { return ColorConverter.ConvertFromString("DimGray"); }
        }

        public static object ControlHoverTextColor {
            get { return ColorConverter.ConvertFromString("#777779"); }
        }

        public static object ControlDisabledColor {
            get { return ColorConverter.ConvertFromString("DimGray"); }
        }

        public static object ControlSelectedUnfocusedColor {
            get { return ColorConverter.ConvertFromString("#515153"); }
        }

        public static object PaperBackgroundColor {
            get { return ColorConverter.ConvertFromString("White"); }
        }

        public static object PaperTextColor {
            get { return ColorConverter.ConvertFromString("#141414"); }
        }

        public static object MenuBackgroundColor {
            get { return ColorConverter.ConvertFromString("#141414"); }
        }

        public static object ScrollThumbBackgroundColor {
            get { return ColorConverter.ConvertFromString("#333335"); }
        }

        public static object ScrollThumbHoverColor {
            get { return ColorConverter.ConvertFromString("#444446"); }
        }

        public static object ScrollThumbDragColor {
            get { return ColorConverter.ConvertFromString("#606060"); }
        }

        public static object ScrollBackgroundColor {
            get { return ColorConverter.ConvertFromString("#CCCCCC"); }
        }
    }
}

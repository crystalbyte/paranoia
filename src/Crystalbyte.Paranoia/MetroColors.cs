using System.Windows.Media;

namespace Crystalbyte.Paranoia {
    public static class MetroColors {
        public static object AccentColor {
            get { return ColorConverter.ConvertFromString("#0065B2"); }
        }

        public static object FailureColor {
            get { return ColorConverter.ConvertFromString("Brown"); }
        }

        public static object SuccessColor {
            get { return ColorConverter.ConvertFromString("ForestGreen"); }
        }

        public static object WarningColor {
            get { return ColorConverter.ConvertFromString("Orange"); }
        }

        public static object WindowBackgroundColor {
            get { return ColorConverter.ConvertFromString("#0B0B0C"); }
        }

        public static object ShadowColor {
            get { return ColorConverter.ConvertFromString("Black"); }
        }

        public static object FlyoutBackgroundColor {
            get { return ColorConverter.ConvertFromString("#252527"); }
        }

        public static object ControlBackgroundColor {
            get { return ColorConverter.ConvertFromString("#222224"); }
        }

        public static object SecondaryControlBackgroundColor {
            get { return ColorConverter.ConvertFromString("#121213"); }
        }

        public static object PaperBackgroundColor {
            get { return ColorConverter.ConvertFromString("White"); }
        }

        public static object ControlBorderColor {
            get { return ColorConverter.ConvertFromString("#525252"); }
        }

        public static object HoverColor {
            get { return ColorConverter.ConvertFromString("#333334"); }
        }

        public static object MenuBackgroundColor {
            get { return ColorConverter.ConvertFromString("#141414"); }
        }

        public static object NormalTextColor {
            get { return ColorConverter.ConvertFromString("GhostWhite"); }
        }

        public static object PaperTextColor {
            get { return ColorConverter.ConvertFromString("#141414"); }
        }

        public static object SecondaryTextColor {
            get { return ColorConverter.ConvertFromString("DimGray"); }
        }
        
        public static object HoverTextColor {
            get { return ColorConverter.ConvertFromString("#777779"); }
        }

        public static object DisabledColor {
            get { return ColorConverter.ConvertFromString("DimGray"); }
        }

        public static object SelectedUnfocusedColor {
            get { return ColorConverter.ConvertFromString("#515153"); }
        }

        public static object ScrollBackgroundColor {
            get { return ColorConverter.ConvertFromString("#333335"); }
        }

        public static object ScrollHoverColor {
            get { return ColorConverter.ConvertFromString("#444446"); }
        }
        
        public static object ScrollDragColor {
            get { return ColorConverter.ConvertFromString("#606060"); }
        }
    }
}

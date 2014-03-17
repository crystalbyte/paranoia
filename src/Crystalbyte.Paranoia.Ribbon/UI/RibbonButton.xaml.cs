using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Crystalbyte.Paranoia.UI {
    public class RibbonButton : Button, IQuickAccessConform {

        #region Construction

        static RibbonButton() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonButton),
               new FrameworkPropertyMetadata(typeof(RibbonButton)));
        }

        public RibbonButton() {
            QuickAccessRegistry.Register(this);
        }

        #endregion

        #region Dependency Properties

        public ImageSource ImageSource {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(RibbonButton), new PropertyMetadata(null));

        #endregion

        #region Implementation of IQuickAccessConform

        public ImageSource QuickAccessImageSource {
            get { return ImageSource; }
        }

        ICommand IQuickAccessConform.Command {
            get { return Command; }
        }

        object IQuickAccessConform.CommandParameter {
            get { return CommandParameter; }
        }

        object IQuickAccessConform.ToolTip {
            get { return ToolTip; }
        }

        public string Key {
            get {
                var routed = Command as RoutedCommand;
                return routed != null ? routed.Name : null;
            }
        }

        #endregion
    }
}

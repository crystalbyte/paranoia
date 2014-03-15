using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Crystalbyte.Paranoia.UI {
    public class RibbonButton : Button, IQuickAccessConform {

        #region Construction

        static RibbonButton() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonButton),
               new FrameworkPropertyMetadata(typeof(RibbonButton)));
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

        ImageSource IQuickAccessConform.QuickAccessImageSource {
            get { return ImageSource; }
        }

        System.Windows.Input.ICommand IQuickAccessConform.Command {
            get { return Command; }
        }

        object IQuickAccessConform.CommandParameter {
            get { return CommandParameter; }
        }

        object IQuickAccessConform.Tooltip {
            get { return ToolTip; }
        }

        #endregion
    }
}

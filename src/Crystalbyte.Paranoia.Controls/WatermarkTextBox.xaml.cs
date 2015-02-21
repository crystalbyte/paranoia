using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for WatermarkTextBox.xaml
    /// </summary>
    public class WatermarkTextBox : TextBox {

        #region Construction

        static WatermarkTextBox() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WatermarkTextBox),
                new FrameworkPropertyMetadata(typeof(WatermarkTextBox)));
        }

        public WatermarkTextBox() {
            TextChanged += OnTextChanged;
        }

        #endregion

        #region Class Overrides

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            InvalidateWatermark();
        }

        #endregion

        #region Methods

        private void OnTextChanged(object sender, TextChangedEventArgs e) {
            InvalidateWatermark();
        }

        private void InvalidateWatermark() {
            IsWatermarkVisible = string.IsNullOrEmpty(Text);
        }

        #endregion

        #region Dependency Properties

        public object Watermark {
            get { return GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark", typeof(object), typeof(WatermarkTextBox), new PropertyMetadata(null));


        public DataTemplate WatermarkTemplate {
            get { return (DataTemplate)GetValue(WatermarkTemplateProperty); }
            set { SetValue(WatermarkTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WatermarkTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WatermarkTemplateProperty =
            DependencyProperty.Register("WatermarkTemplate", typeof(DataTemplate), typeof(WatermarkTextBox), new PropertyMetadata(null));

        public Brush AccentBrush {
            get { return (Brush)GetValue(AccentBrushProperty); }
            set { SetValue(AccentBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AccentBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AccentBrushProperty =
            DependencyProperty.Register("AccentBrush", typeof(Brush), typeof(WatermarkTextBox), new PropertyMetadata(null));

        public bool IsWatermarkVisible {
            get { return (bool)GetValue(IsWatermarkVisibleProperty); }
            set { SetValue(IsWatermarkVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsWatermarkVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsWatermarkVisibleProperty =
            DependencyProperty.Register("IsWatermarkVisible", typeof(bool), typeof(WatermarkTextBox), new PropertyMetadata(false));

        #endregion
    }
}

#region Using directives

using System.Windows;
using System.Windows.Controls;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public static class Watermark {
        public static string GetText(DependencyObject obj) {
            return (string) obj.GetValue(TextProperty);
        }

        public static void SetText(DependencyObject obj, string value) {
            obj.SetValue(TextProperty, value);
        }

        public static bool GetIsEnabled(DependencyObject obj) {
            return (bool) obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value) {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled",
                typeof (bool), typeof (Watermark),
                new UIPropertyMetadata(false, OnIsWatermarkEnabled));


        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached("Text",
                typeof (string), typeof (Watermark),
                new UIPropertyMetadata(string.Empty, OnWatermarkTextChanged));

        private static void OnWatermarkTextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var tb = sender as TextBox;
            if (tb != null) {
                tb.Text = (string) e.NewValue;
            }
        }

        private static void OnIsWatermarkEnabled(object sender, DependencyPropertyChangedEventArgs e) {
            var tb = sender as TextBox;
            if (tb == null)
                return;

            var isEnabled = (bool) e.NewValue;
            if (isEnabled) {
                tb.Loaded += OnInputTextBoxLoaded;
                tb.GotFocus += OnInputTextBoxGotFocus;
                tb.LostFocus += OnInputTextBoxLostFocus;
            }
            else {
                tb.Loaded -= OnInputTextBoxLoaded;
                tb.GotFocus -= OnInputTextBoxGotFocus;
                tb.LostFocus -= OnInputTextBoxLostFocus;
            }
        }

        private static void OnInputTextBoxLoaded(object sender, RoutedEventArgs e) {
            var tb = e.OriginalSource as TextBox;
            if (tb == null)
                return;

            if (string.IsNullOrEmpty(tb.Text))
                tb.Text = GetText(tb);
        }

        private static void OnInputTextBoxLostFocus(object sender, RoutedEventArgs e) {
            var tb = e.OriginalSource as TextBox;
            if (tb == null)
                return;

            if (string.IsNullOrEmpty(tb.Text))
                tb.Text = GetText(tb);
        }

        private static void OnInputTextBoxGotFocus(object sender, RoutedEventArgs e) {
            var tb = e.OriginalSource as TextBox;
            if (tb == null)
                return;

            if (tb.Text == GetText(tb))
                tb.Text = string.Empty;
        }
    }
}
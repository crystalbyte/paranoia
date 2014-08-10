#region Using directives

using System.Windows;
using System.Windows.Controls;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public static class RichTextBoxExtensions {
        public static readonly DependencyProperty BindableTextProperty =
            DependencyProperty.RegisterAttached("BindableText", typeof(string), typeof(RichTextBoxExtensions),
                new UIPropertyMetadata(null, BindableTextPropertyChanged));

        public static string GetBindableText(DependencyObject obj) {
            return (string)obj.GetValue(BindableTextProperty);
        }

        public static void SetBindableText(DependencyObject obj, string value) {
            obj.SetValue(BindableTextProperty, value);
        }

        public static void BindableTextPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
            var textbox = o as RichTextBox;
            if (textbox == null) return;

            try {
                textbox.BeginChange();

                var text = e.NewValue as string;
                if (string.IsNullOrEmpty(text)) {
                    if (textbox.CaretPosition.Paragraph != null)
                        textbox.CaretPosition.Paragraph.Inlines.Clear();
                    return;
                }

                if (textbox.CaretPosition.Paragraph == null) {
                    return;
                }

                textbox.CaretPosition.Paragraph.Inlines.Clear();
                textbox.CaretPosition.Paragraph.Inlines.Add(text);
            }
            finally {
                textbox.EndChange();    
            }
        }
    }
}
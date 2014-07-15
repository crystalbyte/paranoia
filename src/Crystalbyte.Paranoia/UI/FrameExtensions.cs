#region Using directives

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public static class FrameExtensions {
        public static readonly DependencyProperty BindableSourceProperty =
            DependencyProperty.RegisterAttached("BindableSource", typeof (string), typeof (FrameExtensions),
                new UIPropertyMetadata(null, BindableSourcePropertyChanged));

        public static string GetBindableSource(DependencyObject obj) {
            return (string) obj.GetValue(BindableSourceProperty);
        }

        public static void SetBindableSource(DependencyObject obj, string value) {
            obj.SetValue(BindableSourceProperty, value);
        }

        public static void BindableSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
            var frame = o as Frame;
            if (frame == null) return;

            var file = Path.GetTempFileName() + ".html";
            var text = e.NewValue as string;
            File.WriteAllText(file, text ?? string.Empty);
            frame.Navigate(new Uri(string.Format("file:///{0}", file), UriKind.Absolute));
        }
    }
}
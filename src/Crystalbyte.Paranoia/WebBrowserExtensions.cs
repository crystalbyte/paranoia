#region Using directives

using System.Windows;
using System.Windows.Controls;

#endregion

namespace Crystalbyte.Paranoia {
    public static class WebBrowserExtentions {
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.RegisterAttached("Document", typeof (string), typeof (WebBrowserExtentions),
                                                new UIPropertyMetadata(null, DocumentPropertyChanged));

        public static string GetDocument(DependencyObject element) {
            return (string) element.GetValue(DocumentProperty);
        }

        public static void SetDocument(DependencyObject element, string value) {
            element.SetValue(DocumentProperty, value);
        }

        public static void DocumentPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e) {
            var browser = target as WebBrowser;
            if (browser == null)
                return;
            var document = e.NewValue as string;
            if (document != null)
                browser.NavigateToString(document);
        }
    }
}
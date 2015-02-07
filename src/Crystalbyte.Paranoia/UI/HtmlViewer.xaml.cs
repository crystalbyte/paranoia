using System;
using System.Windows;
using System.Windows.Controls;
using CefSharp;
using CefSharp.Wpf;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for HtmlViewer.xaml
    /// </summary>
    [TemplatePart(Name = WebBrowserTemplatePart, Type = typeof(ChromiumWebBrowser))]
    public class HtmlViewer : Control {

        #region Xaml Support

        private const string WebBrowserTemplatePart = "PART_WebBrowser";

        #endregion

        #region Construction

        public HtmlViewer() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HtmlViewer),
                new FrameworkPropertyMetadata(typeof(HtmlViewer)));
        }

        #endregion

        #region Private Fields

        private ChromiumWebBrowser _browser;

        #endregion

        #region Class Overrides

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            _browser = (ChromiumWebBrowser)Template.FindName(WebBrowserTemplatePart, this);
            _browser.BrowserSettings = new BrowserSettings {
                ApplicationCacheDisabled = true,
                JavaDisabled = true,
                WebSecurityDisabled = true,
                WebGlDisabled = true,
                UniversalAccessFromFileUrlsAllowed = true,
                PluginsDisabled = true,
                JavaScriptOpenWindowsDisabled = true,
                JavaScriptCloseWindowsDisabled = true,
                JavascriptDisabled = true
            };
        }

        #endregion

        #region Dependency Property

        public string Source {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(string), typeof(HtmlViewer), new PropertyMetadata(OnSourcePropertyChanged));

        private static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var viewer = (HtmlViewer)d;

            var url = (string)e.NewValue;
            if (string.IsNullOrEmpty(url)) {
                url = "about:blank";
            }

            viewer.Navigate(new Uri(url));
        }

        public double Zoom {
            get { return (double)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Zoom.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("Zoom", typeof(double), typeof(HtmlViewer), new PropertyMetadata(0.0d, OnZoomChanged));

        private static void OnZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var viewer = (HtmlViewer)d;
            var change = (double) e.NewValue/100.0d - 1;
            viewer.ChangeZoom(change);
        }

        #endregion

        #region Methods

        private void ChangeZoom(double level) {
            if (_browser == null) {
                return;
            }

            _browser.ZoomLevel = level;
            _browser.Reload(false);
        }

        private void Navigate(Uri uri) {
            if (_browser == null) {
                return;
            }

            _browser.WebBrowser.Load(uri.AbsoluteUri);
        }

        #endregion
    }
}

using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CefSharp;
using CefSharp.Wpf;
using NLog;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for HtmlEditor.xaml
    /// </summary>
    public class HtmlEditor : Control, IRequestAware {

        #region Xaml Support

        private const string WebBrowserTemplatePart = "PART_WebBrowser";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Private Fields

        private ChromiumWebBrowser _browser;

        #endregion

        #region Construction

        static HtmlEditor() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HtmlEditor),
                new FrameworkPropertyMetadata(typeof(HtmlEditor)));
        }

        public HtmlEditor() {
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, OnCopy));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, OnCut));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, OnPaste));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.SelectAll, OnSelectAll));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Print, OnPrint));
            CommandBindings.Add(new CommandBinding(HtmlCommands.ViewSource, OnViewSource));
        }

        private void OnViewSource(object sender, ExecutedRoutedEventArgs e) {
            try {
                _browser.ViewSource();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnPaste(object sender, ExecutedRoutedEventArgs e) {
            try {
                _browser.Paste();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnCut(object sender, ExecutedRoutedEventArgs e) {
            try {
                _browser.Cut();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        #endregion

        #region Methods

        internal async Task PrintAsync() {
            var browser = new WebBrowser();
            browser.Navigated += (x, y) => {
                try {
                    dynamic document = browser.Document;
                    document.execCommand("print", true, null);
                } catch (Exception ex) {
                    Logger.Error(ex);
                } finally {
                    browser.Dispose();
                }
            };
            var html = await _browser.GetSourceAsync();
            browser.NavigateToString(html);
        }

        private void OnCopy(object sender, ExecutedRoutedEventArgs e) {
            try {
                _browser.Copy();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnSelectAll(object sender, ExecutedRoutedEventArgs e) {
            try {
                _browser.SelectAll();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private async void OnPrint(object sender, ExecutedRoutedEventArgs e) {
            try {
                await PrintAsync();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        #endregion

        #region Class Overrides

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            _browser = (ChromiumWebBrowser)Template.FindName(WebBrowserTemplatePart, this);
            _browser.RequestHandler = new HtmlRequestHandler(this);
            _browser.BrowserSettings = new BrowserSettings {
                DefaultEncoding = Encoding.UTF8.WebName,
                ApplicationCacheDisabled = true,
                JavaDisabled = true,
                WebSecurityDisabled = true,
                WebGlDisabled = true,
                UniversalAccessFromFileUrlsAllowed = true,
                FileAccessFromFileUrlsAllowed = true,
                PluginsDisabled = true,
                JavaScriptOpenWindowsDisabled = false,
                JavaScriptCloseWindowsDisabled = false,
                JavascriptDisabled = false,
                TextAreaResizeDisabled = true
            };

            _browser.RegisterJsObject("extern", new ScriptingObject(this));
            _browser.Load(Source);
        }

        #endregion

        #region Dependency Property

        public string Source {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(string), typeof(HtmlEditor), new PropertyMetadata(OnSourcePropertyChanged));

        private static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var viewer = (HtmlEditor)d;

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
            DependencyProperty.Register("Zoom", typeof(double), typeof(HtmlEditor), new PropertyMetadata(0.0d, OnZoomChanged));

        private static void OnZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var viewer = (HtmlEditor)d;
            var change = (double)e.NewValue / 100.0d;
            viewer.ChangeZoom(change);
        }

        #endregion

        #region Methods

        private void ChangeZoom(double level) {
            if (_browser == null || _browser.WebBrowser == null) {
                return;
            }

            _browser.ZoomLevel = level;
            _browser.Reload(false);
        }

        private void Navigate(Uri uri) {
            if (_browser == null || _browser.WebBrowser == null) {
                return;
            }

            _browser.Load(uri.AbsoluteUri);
        }

        #endregion
    }
}

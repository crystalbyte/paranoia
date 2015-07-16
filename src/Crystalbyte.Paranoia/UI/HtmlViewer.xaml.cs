#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia
// 
// Crystalbyte.Paranoia is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License.
// 
// Foobar is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Using Directives

using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CefSharp;
using CefSharp.Wpf;
using NLog;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for HtmlViewer.xaml
    /// </summary>
    [TemplatePart(Name = WebBrowserTemplatePart, Type = typeof(ChromiumWebBrowser))]
    public class HtmlViewer : Control, IRequestAware {

        #region Xaml Support

        private const string WebBrowserTemplatePart = "PART_WebBrowser";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Private Fields

        private ChromiumWebBrowser _browser;

        #endregion

        #region Construction

        static HtmlViewer() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HtmlViewer),
                new FrameworkPropertyMetadata(typeof(HtmlViewer)));
        }

        public HtmlViewer() {
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, OnCopy));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.SelectAll, OnSelectAll));
            CommandBindings.Add(new CommandBinding(HtmlCommands.ViewSource, OnViewSource));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Print, OnPrint));
        }

        private void OnViewSource(object sender, ExecutedRoutedEventArgs e) {
            try {
                _browser.ViewSource();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        #endregion

        #region Events 

        public event EventHandler Ready;

        protected virtual void OnReady() {
            var handler = Ready;
            if (handler != null) 
                handler(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        internal async Task PrintAsync() {
            throw new NotImplementedException("Printing does not work.");
            var browser = new WebBrowser();
            browser.LoadCompleted += (x, y) => {
                try {
                    dynamic document = browser.Document;
                    document.execCommand("print", true, null);
                } catch (Exception ex) {
                    Logger.ErrorException(ex.Message, ex);
                } finally {
                    browser.Dispose();
                }
            };
            var html = await _browser.GetSourceAsync();
            browser.NavigateToString(html);
        }

        private void OnSelectAll(object sender, ExecutedRoutedEventArgs e) {
            try {
                _browser.SelectAll();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private async void OnPrint(object sender, ExecutedRoutedEventArgs e) {
            try {
                await PrintAsync();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnCopy(object sender, ExecutedRoutedEventArgs e) {
            try {
                _browser.Copy();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
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
                JavaScriptOpenWindowsDisabled = true,
                JavaScriptCloseWindowsDisabled = true,
                JavascriptDisabled = true
            };

            _browser.IsBrowserInitializedChanged += OnIsBrowserInitializedChanged;
        }

        private void OnIsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e) {
            try {
                OnReady();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        #endregion

        #region Dependency Property

        public string Source {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(string), typeof(HtmlViewer),
                new PropertyMetadata(OnSourcePropertyChanged));

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
            DependencyProperty.Register("Zoom", typeof(double), typeof(HtmlViewer),
                new PropertyMetadata(0.0d, OnZoomChanged));

        private static void OnZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var viewer = (HtmlViewer)d;
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
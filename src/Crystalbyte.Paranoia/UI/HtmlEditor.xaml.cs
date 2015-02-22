using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CefSharp;
using CefSharp.Wpf;
using Newtonsoft.Json;
using NLog;
using Control = System.Windows.Controls.Control;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using WebBrowser = System.Windows.Controls.WebBrowser;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for HtmlEditor.xaml
    /// </summary>
    [TemplatePart(Name = WebBrowserTemplatePart, Type = typeof(ChromiumWebBrowser))]
    [TemplatePart(Name = EditorMenuBorderTemplatePart, Type = typeof(Border))]
    public class HtmlEditor : Control, IRequestAware {

        #region Xaml Support

        private const string WebBrowserTemplatePart = "PART_WebBrowser";
        private const string EditorMenuBorderTemplatePart = "PART_EditorMenuBorder";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Private Fields

        private ChromiumWebBrowser _browser;
        private Border _editorBorder;

        #endregion

        #region Construction

        static HtmlEditor() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HtmlEditor),
                new FrameworkPropertyMetadata(typeof(HtmlEditor)));
        }

        public HtmlEditor() {
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, OnCopy));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, OnUndo));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo, OnRedo));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, OnCut));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, OnPaste));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.SelectAll, OnSelectAll));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Print, OnPrint));
            CommandBindings.Add(new CommandBinding(HtmlCommands.ViewSource, OnViewSource));
            CommandBindings.Add(new CommandBinding(EditingCommands.ToggleStrikethrough, OnToggleStrikethrough));
        }

        #endregion

        #region Events

        public event EventHandler ContentChanged;

        protected virtual void OnContentChanged() {
            var handler = ContentChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion

        #region Quill Interop

        /// <summary>
        /// Wrapper methods for the Quill editor, see http://quilljs.com/docs/api/.
        /// Check formats: http://quilljs.com/docs/formats/
        /// </summary>

        public void Undo() {
            const string script = "(function() { quill.modules['undo-manager'].undo(); })();";
            _browser.ExecuteScriptAsync(script);
            _browser.Focus();
        }

        public void Redo() {
            const string script = "(function() { quill.modules['undo-manager'].redo(); })();";
            _browser.ExecuteScriptAsync(script);
            _browser.Focus();
        }

        public async Task SetStrikethrough(bool strike) {
            await SetFormatAsync("strike", strike);
        }

        public async Task SetUnderline(bool underline) {
            await SetFormatAsync("underline", underline);
        }

        public async Task SetTextColorAsync(Color color) {
            await SetFormatAsync("color", color.ToHex(false));
        }

        public async Task SetBackgroundColorAsync(Color color) {
            await SetFormatAsync("background", color.ToHex(false));
        }

        public async Task SetBoldAsync(bool bold) {
            await SetFormatAsync("bold", bold);
        }

        public async Task SetItalicAsync(bool italic) {
            await SetFormatAsync("italic", italic);
        }

        public async Task SetFontSizeAsync(int size) {
            await SetFormatAsync("size", string.Format("{0}px", size));
        }

        public async Task SetFontFamilyAsync(FontFamily family) {
            await SetFormatAsync("font", family.Source);
        }

        public async Task SetLinkAsync(string url) {
            await SetFormatAsync("link", url);
        }

        public async Task SetFormatAsync(string format, object value) {
            var range = await GetSelectionAsync();
            if (range.IsPosition) {
                PrepareFormat(format, value);
            } else {
                FormatText(range.Start, range.End, format, value);
            }
        }

        public void SetText(string text) {
            var script = string.Format("(function() {{ quill.setText('{0}'); }})();", text);
            _browser.ExecuteScriptAsync(script);
            _browser.Focus();
        }

        public void SetHtml(string html) {
            var script = string.Format("(function() {{ quill.setHtml('{0}'); }})();", html);
            _browser.ExecuteScriptAsync(script);
            _browser.Focus();
        }

        public void SetContents(string content) {
            var script = string.Format("(function() {{ quill.setContent('{0}'); }})();", content);
            _browser.ExecuteScriptAsync(script);
            _browser.Focus();
        }

        private void SetFocus() {
            var script = string.Format("(function() {{ quill.focus(); }})();");
            _browser.ExecuteScriptAsync(script);
        }

        public void InsertText(int index, string text) {
            var script = string.Format("(function() {{ quill.insertText({0}, '{1}'); }})();", index, text);
            _browser.ExecuteScriptAsync(script);
            _browser.Focus();
        }

        public void InsertEmbed(int index, string type, string url) {
            var script = string.Format("(function() {{ quill.insertEmbed({0}, '{1}', '{2}'); }})();", index, type, url);
            _browser.ExecuteScriptAsync(script);
            _browser.Focus();
        }

        private void FormatText(int start, int end, string name, object value) {
            if (value is string) {
                value = string.Format("'{0}'", value.ToString().ToLower());
            } else {
                value = value.ToString().ToLower();
            }
            var script = string.Format("(function() {{ quill.formatText({0}, {1}, '{2}', {3}); }})();", start, end, name, value);
            _browser.ExecuteScriptAsync(script);
            _browser.Focus();
        }

        private void PrepareFormat(string format, object value) {
            if (value is string) {
                value = string.Format("'{0}'", value.ToString().ToLower());
            } else {
                value = value.ToString().ToLower();
            }
            var script = string.Format("(function() {{ quill.prepareFormat('{0}', {1}); }})();", format, value);
            _browser.ExecuteScriptAsync(script);
            _browser.Focus();
        }

        public void InsertHtml(int index, string text) {
            var script = string.Format("(function() {{ return quill.insertHtml({0}, '{1}'); }})();", index, text);
            _browser.ExecuteScriptAsync(script);
            _browser.Focus();
        }

        public async Task<int> GetLengthAsync() {
            const string script = "(function() { return quill.getLength(); })();";
            var response = await _browser.EvaluateScriptAsync(script);

            if (!response.Success) {
                throw new ScriptingException(response.Message);
            }

            return (int)response.Result;
        }

        public async Task<TextRange> GetSelectionAsync() {
            const string script = "(function() { var s = quill.getSelection(); return JSON.stringify(s); })();";
            var response = await _browser.EvaluateScriptAsync(script);

            if (!response.Success) {
                return TextRange.FromPosition(0);
            }

            var json = response.Result as string;
            return string.IsNullOrEmpty(json) || string.Compare("null", json, StringComparison.OrdinalIgnoreCase) == 0
                ? TextRange.FromPosition(0)
                : JsonConvert.DeserializeObject<TextRange>(json);
        }

        public async Task<string> GetHtmlAsync() {
            const string script = "(function() { return quill.getHTML(); })();";
            var response = await _browser.EvaluateScriptAsync(script);

            if (!response.Success) {
                throw new ScriptingException(response.Message);
            }

            return response.Result as string;
        }

        public async Task<string> GetContentsAsync() {
            const string script = "(function() { return quill.getContents(); })();";
            var response = await _browser.EvaluateScriptAsync(script);

            if (!response.Success) {
                throw new ScriptingException(response.Message);
            }

            return response.Result as string;
        }

        public async Task<IDictionary<string, object>> GetContentsAsync(int start, int end) {
            var script = string.Format("(function() {{ return quill.getContents({0}, {1}); }})();", start, end);
            var response = await _browser.EvaluateScriptAsync(script);

            if (!response.Success) {
                throw new ScriptingException(response.Message);
            }

            return response.Result as IDictionary<string, object>;
        }

        public async Task<string> GetTextAsync() {
            const string script = "(function() { return quill.getText(); })();";
            var response = await _browser.EvaluateScriptAsync(script);

            if (!response.Success) {
                throw new ScriptingException(response.Message);
            }

            return response.Result as string;
        }

        #endregion

        #region Methods

        private void OnToggleStrikethrough(object sender, ExecutedRoutedEventArgs e) {
            ToggleStrikethrough();
        }

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

        public async Task InsertImageAsync() {
            try {
                var dialog = new OpenFileDialog {
                    DefaultExt = ".png",
                    Filter = string.Format("{0}|*.jpeg;*.png;*.jpg;*.gif|" +
                                           "{1}|*.png|" +
                                           "{2}|*.jpeg;*.jpg|" +
                                           "{3}|*.gif",
                                           Paranoia.Properties.Resources.AllImages,
                                           Paranoia.Properties.Resources.PngFiles,
                                           Paranoia.Properties.Resources.JpgFiles,
                                           Paranoia.Properties.Resources.GifFiles)
                };

                // Display OpenFileDialog by calling ShowDialog method 
                var result = dialog.ShowDialog();
                if (!(result.HasValue && result.Value)) {
                    return;
                }

                if (!Selection.HasValue) {
                    var length = await GetLengthAsync();
                    Selection = TextRange.FromPosition(length - 1);
                }

                var url = string.Format("file:///{0}", WebUtility.UrlEncode(dialog.FileName));
                InsertEmbed(Selection.Value.Start, "image", url);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnRedo(object sender, ExecutedRoutedEventArgs e) {
            if (_browser == null) {
                return;
            }

            _browser.Redo();
        }

        private void OnUndo(object sender, ExecutedRoutedEventArgs e) {
            if (_browser == null) {
                return;
            }

            _browser.Undo();
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

        public async Task UpdateSelectionAsync() {
            Selection = await GetSelectionAsync();
        }

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

            _editorBorder = (Border)Template.FindName(EditorMenuBorderTemplatePart, this);
            _editorBorder.DataContext = new HtmlEditorCommandContext(this);

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

            _browser.GotKeyboardFocus += (sender, e) => SetFocus();
            _browser.RegisterJsObject("extern", new ScriptingObject(this));
            _browser.Load(Source);
        }

        #endregion

        #region Dependency Property

        public string Content {
            get { return (string)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(string), typeof(HtmlEditor), new PropertyMetadata(string.Empty));

        public TextRange? Selection {
            get { return (TextRange?)GetValue(SelectionProperty); }
            set { SetValue(SelectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Selection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionProperty =
            DependencyProperty.Register("Selection", typeof(TextRange?), typeof(HtmlEditor), new PropertyMetadata(null));

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

        public async Task InvalidateCommandsAsync() {
            var context = (HtmlEditorCommandContext) _editorBorder.DataContext;
            await context.InvalidateAsync();
        }

        public void ToggleStrikethrough() {
            var context = (HtmlEditorCommandContext)_editorBorder.DataContext;
            context.IsStrikethrough = !context.IsStrikethrough;
        }
    }
}

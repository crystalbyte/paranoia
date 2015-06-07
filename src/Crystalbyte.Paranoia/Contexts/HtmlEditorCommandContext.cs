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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI;
using Crystalbyte.Paranoia.UI.Commands;
using NLog;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class HtmlEditorCommandContext : NotificationObject {
        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly HtmlEditor _editor;
        private bool _isBold;
        private bool _isItalic;
        private bool _isUnderlined;
        private bool _isStrikethrough;
        private Color _textColor;
        private Color _backgroundColor;
        private FontFamily _fontFamily;
        private int _fontSize;
        private TextAlignment _textAlignment;

        #endregion

        public HtmlEditorCommandContext(HtmlEditor editor) {
            _editor = editor;
            _fontSize = Settings.Default.DefaultWebFontSize;
            _fontFamily = new FontFamily(Settings.Default.DefaultWebFont);

            Attributes = new Dictionary<string, object>();
            UndoCommand = new RelayCommand(OnCanUndo, OnUndo);
            RedoCommand = new RelayCommand(OnCanRedo, OnRedo);
            ListCommand = new RelayCommand(OnCanList, OnList);
            BulletCommand = new RelayCommand(OnCanBullet, OnBullet);
            LinkCommand = new RelayCommand(OnCanLink, OnLink);
            ImageCommand = new RelayCommand(OnCanImage, OnImage);
        }

        private bool OnCanImage(object obj) {
            return true;
        }

        private async void OnImage(object obj) {
            await _editor.InsertImageAsync();
        }

        private void OnLink(object obj) {}

        private bool OnCanLink(object obj) {
            return true;
        }

        private void OnBullet(object obj) {}

        private bool OnCanBullet(object obj) {
            return true;
        }

        private bool OnCanList(object obj) {
            return true;
        }

        private void OnList(object obj) {}

        private void OnRedo(object obj) {
            try {
                _editor.Redo();
            }
            catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private static bool OnCanRedo(object obj) {
            return true;
        }

        private void OnUndo(object obj) {
            try {
                _editor.Undo();
            }
            catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private async void OnFontFamilyChanged() {
            await _editor.SetFontFamilyAsync(FontFamily);
        }

        private static bool OnCanUndo(object obj) {
            return true;
        }

        public IEnumerable<Color> TextColors {
            get {
                var colors = Settings.Default.TextFontColors.OfType<string>().OrderBy(x => x);
                return colors.Select(ColorConverter.ConvertFromString).Where(color => color != null).Cast<Color>();
            }
        }

        public IEnumerable<Color> BackgroundColors {
            get {
                var colors =
                    Settings.Default.BackgroundFontColors.OfType<string>()
                        .Concat(new[] {"Transparent"})
                        .OrderByDescending(x => x);
                return colors.Select(ColorConverter.ConvertFromString).Where(color => color != null).Cast<Color>();
            }
        }

        public IEnumerable<FontFamily> FontFamilies {
            get {
                var families = Settings.Default.WebFonts.OfType<string>().OrderBy(x => x);
                return families.Select(x => new FontFamily(x));
            }
        }

        public IEnumerable<int> FontSizes {
            // "10px", "13px", "16px", "18px", "24px", "32px", "48px"
            get { return new[] {10, 13, 16, 24, 48}; }
        }

        public bool IsBold {
            get { return _isBold; }
            set {
                if (_isBold == value) {
                    return;
                }
                _isBold = value;
                RaisePropertyChanged(() => IsBold);
                OnBoldChanged();
            }
        }

        private async void OnBoldChanged() {
            await _editor.SetBoldAsync(IsBold);
        }

        public bool IsItalic {
            get { return _isItalic; }
            set {
                if (_isItalic == value) {
                    return;
                }
                _isItalic = value;
                RaisePropertyChanged(() => IsItalic);
                OnItalicChanged();
            }
        }

        private async void OnItalicChanged() {
            await _editor.SetItalicAsync(IsItalic);
        }

        public bool IsUnderlined {
            get { return _isUnderlined; }
            set {
                if (_isUnderlined == value) {
                    return;
                }
                _isUnderlined = value;
                RaisePropertyChanged(() => IsUnderlined);
                OnUnderlineChanged();
            }
        }

        private async void OnUnderlineChanged() {
            await _editor.SetUnderline(IsUnderlined);
        }

        public bool IsStrikethrough {
            get { return _isStrikethrough; }
            set {
                if (_isStrikethrough == value) {
                    return;
                }
                _isStrikethrough = value;
                RaisePropertyChanged(() => IsStrikethrough);
                OnStrikethroughChanged();
            }
        }

        private async void OnStrikethroughChanged() {
            if (IsEditorStrikethrough) {
                return;
            }
            await _editor.SetStrikethrough(IsStrikethrough);
        }

        public Color TextColor {
            get { return _textColor; }
            set {
                if (_textColor == value) {
                    return;
                }
                _textColor = value;
                RaisePropertyChanged(() => TextColor);
                OnTextColorChanged();
            }
        }

        private async void OnTextColorChanged() {
            await _editor.SetTextColorAsync(TextColor);
        }

        public Color BackgroundColor {
            get { return _backgroundColor; }
            set {
                if (_backgroundColor == value) {
                    return;
                }
                _backgroundColor = value;
                RaisePropertyChanged(() => BackgroundColor);
                OnBackgroundColorChanged();
            }
        }

        private async void OnBackgroundColorChanged() {
            await _editor.SetBackgroundColorAsync(BackgroundColor);
        }

        public FontFamily FontFamily {
            get { return _fontFamily; }
            set {
                if (Equals(_fontFamily, value)) {
                    return;
                }
                _fontFamily = value;
                RaisePropertyChanged(() => FontFamily);
                OnFontFamilyChanged();
            }
        }

        public int FontSize {
            get { return _fontSize; }
            set {
                if (_fontSize == value) {
                    return;
                }
                _fontSize = value;
                RaisePropertyChanged(() => FontSize);
                OnFontSizeChanged();
            }
        }

        private async void OnFontSizeChanged() {
            await _editor.SetFontSizeAsync(FontSize);
        }

        public TextAlignment TextAlignment {
            get { return _textAlignment; }
            set {
                if (_textAlignment == value) {
                    return;
                }
                _textAlignment = value;
                RaisePropertyChanged(() => TextAlignment);
            }
        }

        public RelayCommand UndoCommand { get; private set; }

        public RelayCommand RedoCommand { get; private set; }

        public RelayCommand ListCommand { get; private set; }

        public RelayCommand BulletCommand { get; private set; }

        public RelayCommand LinkCommand { get; private set; }

        public RelayCommand ImageCommand { get; private set; }

        public bool IsEditorBold {
            get { return Attributes.ContainsKey("bold") && (bool) Attributes["bold"]; }
        }

        public bool IsEditorItalic {
            get { return Attributes.ContainsKey("italic") && (bool) Attributes["italic"]; }
        }

        public bool IsEditorStrikethrough {
            get { return Attributes.ContainsKey("strike") && (bool) Attributes["strike"]; }
        }

        public bool IsEditorUnderline {
            get { return Attributes.ContainsKey("underline") && (bool) Attributes["underline"]; }
        }

        public async Task InvalidateAsync() {
            await UpdateAttributesAsync();

            UndoCommand.OnCanExecuteChanged();
            RedoCommand.OnCanExecuteChanged();

            _isBold = Attributes.ContainsKey("bold") && (bool) Attributes["bold"];
            RaisePropertyChanged(() => IsBold);

            _isItalic = Attributes.ContainsKey("italic") && (bool) Attributes["italic"];
            RaisePropertyChanged(() => IsItalic);

            _isStrikethrough = Attributes.ContainsKey("strike") && (bool) Attributes["strike"];
            RaisePropertyChanged(() => IsStrikethrough);

            _isUnderlined = Attributes.ContainsKey("underline") && (bool) Attributes["underline"];
            RaisePropertyChanged(() => IsUnderlined);

            var info = CultureInfo.InvariantCulture.TextInfo;

            if (Attributes.ContainsKey("font")) {
                var name = Attributes["font"] as string;
                if (!string.IsNullOrEmpty(name)) {
                    _fontFamily = new FontFamily(info.ToTitleCase(name.Trim('\'')));
                }
            }
            else {
                _fontFamily = new FontFamily(info.ToTitleCase(Settings.Default.DefaultWebFont));
            }
            RaisePropertyChanged(() => FontFamily);

            if (Attributes.ContainsKey("size")) {
                var pixels = Attributes["size"] as string;
                _fontSize = string.IsNullOrEmpty(pixels)
                    ? Settings.Default.DefaultWebFontSize
                    : int.Parse(pixels.Replace("px", string.Empty));
            }
            else {
                _fontSize = Settings.Default.DefaultWebFontSize;
            }
            RaisePropertyChanged(() => FontSize);


            if (Attributes.ContainsKey("color")) {
                var value = (string) Attributes["color"];
                _textColor = value.ToColor();
            }
            else {
                _textColor = Colors.Black;
            }
            RaisePropertyChanged(() => TextColor);

            if (Attributes.ContainsKey("background")) {
                var value = (string) Attributes["background"];
                _backgroundColor = value.ToColor();
            }
            else {
                _backgroundColor = Colors.Transparent;
            }
            RaisePropertyChanged(() => BackgroundColor);
        }

        public IDictionary<string, object> Attributes { get; set; }

        public async Task UpdateAttributesAsync() {
            var range = await _editor.GetSelectionAsync();
            if (range.IsPosition) {
                // Make it a range to capture deltas.
                range.End = range.End + 1;
            }
            var contents = await _editor.GetContentsAsync(range.Start, range.End);

            Attributes = new Dictionary<string, object>();
            if (!contents.ContainsKey("ops")) {
                return;
            }

            var ops = contents["ops"] as object[];
            if (ops == null) {
                return;
            }

            if (ops.Length == 0) {
                return;
            }

            var operations = ops[0] as IDictionary<string, object>;
            if (operations == null || !operations.ContainsKey("attributes")) {
                return;
            }

            Attributes = (operations["attributes"] as IDictionary<string, object>)
                         ?? new Dictionary<string, object>();
        }
    }
}
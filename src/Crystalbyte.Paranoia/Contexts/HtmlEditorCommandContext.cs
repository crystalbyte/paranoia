using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI;
using Crystalbyte.Paranoia.UI.Commands;
using Microsoft.Win32;
using NLog;

namespace Crystalbyte.Paranoia {
    public sealed class HtmlEditorCommandContext : NotificationObject {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly HtmlEditor _editor;
        private bool _isBold;
        private bool _isItalic;
        private bool _isUnderlined;
        private bool _isStrikedThrough;
        private Color _textColor;
        private Color _backgroundColor;
        private FontFamily _fontFamily;
        private HtmlFontSize _fontSize;
        private TextAlignment _textAlignment;

        #endregion

        public HtmlEditorCommandContext(HtmlEditor editor) {
            _editor = editor;
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

        private void OnLink(object obj) {

        }

        private bool OnCanLink(object obj) {
            return true;
        }

        private void OnBullet(object obj) {

        }

        private bool OnCanBullet(object obj) {
            return true;
        }

        private bool OnCanList(object obj) {
            return true;
        }

        private void OnList(object obj) {

        }

        private void OnRedo(object obj) {
            try {
                _editor.Redo();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private bool OnCanRedo(object obj) {
            return true;
        }

        private void OnUndo(object obj) {
            try {
                _editor.Undo();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private async void OnFontFamilyChanged() {
            await _editor.ChangeFontFamilyAsync(FontFamily);
        }

        private bool OnCanUndo(object obj) {
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
                var colors = Settings.Default.BackgroundFontColors.OfType<string>().Concat(new[] { "Transparent" }).OrderByDescending(x => x);
                return colors.Select(ColorConverter.ConvertFromString).Where(color => color != null).Cast<Color>();
            }
        }

        public IEnumerable<FontFamily> FontFamilies {
            get {
                var families = Settings.Default.WebFonts.OfType<string>().OrderBy(x => x);
                return families.Select(x => new FontFamily(x));
            }
        }

        public IEnumerable<HtmlFontSize> FontSizes {
            get { return new[] { HtmlFontSize.Small, HtmlFontSize.Normal, HtmlFontSize.Large, HtmlFontSize.Huge }; }
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
            await _editor.SetBold(IsBold);
        }

        public bool IsItalic {
            get { return _isItalic; }
            set {
                if (_isItalic == value) {
                    return;
                }
                _isItalic = value;
                RaisePropertyChanged(() => IsItalic);
            }
        }

        public bool IsUnderlined {
            get { return _isUnderlined; }
            set {
                if (_isUnderlined == value) {
                    return;
                }
                _isUnderlined = value;
                RaisePropertyChanged(() => IsUnderlined);
            }
        }

        public bool IsStrikedThrough {
            get { return _isStrikedThrough; }
            set {
                if (_isStrikedThrough == value) {
                    return;
                }
                _isStrikedThrough = value;
                RaisePropertyChanged(() => IsStrikedThrough);
            }
        }

        public Color TextColor {
            get { return _textColor; }
            set {
                if (_textColor == value) {
                    return;
                }
                _textColor = value;
                RaisePropertyChanged(() => TextColor);
            }
        }

        public Color BackgroundColor {
            get { return _backgroundColor; }
            set {
                if (_backgroundColor == value) {
                    return;
                }
                _backgroundColor = value;
                RaisePropertyChanged(() => BackgroundColor);
            }
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

        public HtmlFontSize FontSize {
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
            await _editor.ChangeFontSizeAsync(FontSize);
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
    }
}

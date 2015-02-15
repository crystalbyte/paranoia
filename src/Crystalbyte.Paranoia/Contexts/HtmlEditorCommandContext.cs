using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Automation.Text;
using System.Windows.Input;
using System.Windows.Media;
using Crystalbyte.Paranoia.UI;
using Crystalbyte.Paranoia.UI.Commands;

namespace Crystalbyte.Paranoia {
    public sealed class HtmlEditorCommandContext : NotificationObject {
        private readonly HtmlEditor _editor;
        private bool _isBold;
        private bool _isItalic;
        private bool _isUnderlined;
        private bool _isStrikedThrough;
        private Color _textColor;
        private Color _backgroundColor;
        private FontFamily _fontFamily;
        private int _fontSize;
        private TextAlignment _textAlignment;

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

        private void OnImage(object obj) {
            
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

        }

        private bool OnCanRedo(object obj) {
            return true;
        }

        private void OnUndo(object obj) {

        }

        private bool OnCanUndo(object obj) {
            return true;
        }

        public IEnumerable<Color> Colors {
            get {
                var colorType = typeof(System.Drawing.Color);
                var propInfoList = colorType.GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public);
                return propInfoList.Select(c => ColorConverter.ConvertFromString(c.Name)).Where(color => color != null).Cast<Color>();
            }
        }

        public bool IsBold {
            get { return _isBold; }
            set {
                if (_isBold == value) {
                    return;
                }
                _isBold = value;
                RaisePropertyChanged(() => IsBold);
            }
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
            }
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

﻿using System;
using System.Composition;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Crystalbyte.Paranoia.Contexts;
using Crystalbyte.Paranoia.Properties;

namespace Crystalbyte.Paranoia.Commands {
    [Export, Shared]
    [Export(typeof(IAppBarCommand))]
    public sealed class ReplyCommand : IAppBarCommand {

        #region Implementation of IAppBarCommand

        private ImageSource _image;

        #endregion

        #region Import Declarations

        [Import]
        public MailSelectionSource MailSelectionSource { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            MailSelectionSource.SelectionChanged += (sender, e) => OnCanExecuteChanged();
        }

        #endregion

        #region Implementation of IAppBarCommand

        public bool CanExecute(object parameter) {
            return MailSelectionSource.Mails.Count == 1;
        }

        public void Execute(object parameter) {
            Reply();
        }

        private void Reply() {
            MessageBox.Show("Not yet implemented");
        }

        public event EventHandler CanExecuteChanged;

        private void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public string Tooltip {
            get { return Resources.ReplyCommandTooltip; }
        }
        public string Category {
            get { return AppBarCategory.Mails; }
        }
        public ImageSource Image {
            get {
                if (_image != null) return _image;
                var uri =
                    new Uri(string.Format(Pack.Application, typeof(InviteContactCommand).Assembly.FullName,
                        "Assets/back.png"), UriKind.Absolute);
                _image = new BitmapImage(uri);
                return _image;
            }
        }

        public int Position {
            get { return 0; }
        }

        #endregion
    }
}

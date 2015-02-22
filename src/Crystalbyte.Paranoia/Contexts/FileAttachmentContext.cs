using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Crystalbyte.Paranoia.UI.Commands;
using NLog;

namespace Crystalbyte.Paranoia {
    public sealed class FileAttachmentContext {

        #region Private Fields

        private readonly FileInfo _info;
        private readonly RelayCommand _openCommand;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public FileAttachmentContext(string path) {
            _info = new FileInfo(path);
            _openCommand = new RelayCommand(OnOpen);
        }

        #endregion

        #region Methods

        private void OnOpen(object obj) {
            Open();
        }

        internal void Open() {
            try {
                var process = new Process {
                    StartInfo = new ProcessStartInfo(_info.FullName)
                };
                process.Start();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        #endregion

        #region Properties

        public string FullName {
            get { return _info.FullName; }
        }

        public bool IsImage {
            get {
                return _info.Exists &&
                    Regex.IsMatch(FullName, ".jpg|.png|.jpeg|.tiff|.gif", RegexOptions.IgnoreCase);
            }
        }

        public byte[] Bytes {
            get { return File.ReadAllBytes(_info.FullName); }
        }

        public string Name {
            get { return _info.Name; }
        }

        public ICommand OpenCommand {
            get { return _openCommand; }
        }

        #endregion
    }
}

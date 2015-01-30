using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Crystalbyte.Paranoia.Mail.Mime;
using Crystalbyte.Paranoia.UI.Commands;
using Microsoft.Win32;
using NLog;

namespace Crystalbyte.Paranoia {
    public class AttachmentContext {

        private readonly string _name;
        private readonly string _fullname;
        private readonly MessagePart _part;
        private readonly RemoveAttachmentCommand _removeCommand;
        private readonly OpenAttachmentCommand _openCommand;
        private readonly RelayCommand _saveCommand;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public AttachmentContext(MailCompositionContext context, string fullname) {
            _fullname = fullname;
            _name = fullname.Split('\\').Last();
            _saveCommand = new RelayCommand(OnSave);
            _removeCommand = new RemoveAttachmentCommand(context, this);
        }

        private async void OnSave(object obj) {
            var dialog = new SaveFileDialog { FileName = _fullname };
            var result = dialog.ShowDialog();
            if (!result.HasValue || !result.Value) {
                return;
            }

            await Task.Run(() => {
                try {
                    File.WriteAllBytes(dialog.FileName, Bytes);
                } catch (Exception ex) {
                    Logger.Error(ex);
                }
            });
        }

        public AttachmentContext(MessagePart part) {
            _part = part;
            if (!part.IsAttachment)
                throw new InvalidOperationException("part must be an attachment");

            _name = part.FileName;
            _fullname = part.FileName;
            _openCommand = new OpenAttachmentCommand(part);
        }

        public void Open() {
            if (_openCommand != null && _openCommand.CanExecute(null)) {
                _openCommand.Execute(null);
            }
        }

        public string Name {
            get { return _name; }
        }

        public string FullName {
            get { return _fullname; }
        }

        public bool IsImage {
            get {
                if (File.Exists(_fullname)) {
                    return Regex.IsMatch(FullName, ".jpg|.png|.jpeg|.tiff|.gif", RegexOptions.IgnoreCase);
                }

                if (_part == null) {
                    return false;
                }

                return _part.ContentType.MediaType.Contains("image")
                    || Regex.IsMatch(_part.FileName, ".jpg|.png|.jpeg|.tiff|.gif", RegexOptions.IgnoreCase);
            }
        }

        public byte[] Bytes {
            get {
                return File.Exists(FullName) ? File.ReadAllBytes(FullName) : _part.Body;
            }
        }

        public ICommand RemoveCommand {
            get { return _removeCommand; }
        }

        public ICommand OpenCommand {
            get { return _openCommand; }
        }

        public ICommand SaveCommand {
            get { return _saveCommand; }
        }
    }
}

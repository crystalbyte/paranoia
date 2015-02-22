using Crystalbyte.Paranoia.Mail.Mime;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI.Commands {
    public class OpenAttachmentCommand : ICommand {

        private readonly MessagePart _part;

        public OpenAttachmentCommand(MessagePart part) {
            _part = part;
        }
        public bool CanExecute(object parameter) {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public virtual void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void Execute(object parameter) {
            try {
                var tempPath = Path.GetTempPath();
                var a = 1;
                var fileName = _part.FileName;
                while (File.Exists(tempPath + fileName)) {
                    fileName = _part.FileName.Insert(_part.FileName.LastIndexOf(".", StringComparison.Ordinal) - 1, string.Format("{0}", a));
                    a++;
                }
                _part.Save(new FileInfo(tempPath +  fileName));
                var process = new Process { StartInfo = new ProcessStartInfo(tempPath + fileName) };
                process.Start();
                process.Exited += (sender, e) => File.Delete(tempPath + fileName);
            } catch (Exception ex) {
                MessageBox.Show("something went wrong\n" + ex);
            }
        }
    }
}

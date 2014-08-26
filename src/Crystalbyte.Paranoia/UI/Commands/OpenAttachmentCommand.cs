using Crystalbyte.Paranoia.Mail.Mime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public void Execute(object parameter) {

            try {
                var tempPath = Path.GetTempPath();
                int a = 1;
                var fileName = _part.FileName;
                while (File.Exists(tempPath + fileName)) {
                    fileName = _part.FileName.Insert(_part.FileName.LastIndexOf(".") - 1, string.Format("{0}", a));
                }
                _part.Save(new FileInfo(fileName));
                var process = new Process();
                process.StartInfo = new ProcessStartInfo(fileName);
                process.Start();
                process.Exited += (sender, e) => File.Delete(fileName);
            } catch (Exception ex) {
                MessageBox.Show("something went wrong\n" + ex);
            }

        }
    }
}

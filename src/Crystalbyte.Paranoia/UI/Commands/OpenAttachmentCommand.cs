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
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Crystalbyte.Paranoia.Mail.Mime;

#endregion

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
                    fileName = _part.FileName.Insert(_part.FileName.LastIndexOf(".", StringComparison.Ordinal) - 1,
                        string.Format("{0}", a));
                    a++;
                }
                _part.Save(new FileInfo(tempPath + fileName));
                var process = new Process {StartInfo = new ProcessStartInfo(tempPath + fileName)};
                process.Start();
                process.Exited += (sender, e) => File.Delete(tempPath + fileName);
            }
            catch (Exception ex) {
                MessageBox.Show("something went wrong\n" + ex);
            }
        }
    }
}
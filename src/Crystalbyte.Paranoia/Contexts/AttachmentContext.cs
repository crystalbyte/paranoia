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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Crystalbyte.Paranoia.Mail.Mime;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI.Commands;
using Microsoft.Win32;
using NLog;

#endregion

namespace Crystalbyte.Paranoia {
    public class AttachmentContext {
        private readonly string _name;
        private readonly MessagePart _part;
        private readonly OpenAttachmentCommand _openCommand;
        private readonly RelayCommand _saveCommand;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private async void OnSave(object obj) {
            var extension = _name.Split('.').LastOrDefault() ?? string.Empty;
            var dialog = new SaveFileDialog {
                FileName = _name,
                DefaultExt = extension,
                Filter = string.Format("{0} (*.*)|*.*", Resources.AllFiles)
            };

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
            _saveCommand = new RelayCommand(OnSave);
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

        public bool IsImage {
            get {
                if (_part == null) {
                    return false;
                }

                return _part.ContentType.MediaType.Contains("image")
                       || Regex.IsMatch(_part.FileName, ".jpg|.png|.jpeg|.tiff|.gif", RegexOptions.IgnoreCase);
            }
        }

        public byte[] Bytes {
            get { return _part.Body; }
        }


        public ICommand OpenCommand {
            get { return _openCommand; }
        }

        public ICommand SaveCommand {
            get { return _saveCommand; }
        }
    }
}
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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI.Commands;
using Microsoft.Win32;
using NLog;

#endregion

namespace Crystalbyte.Paranoia {
    public class MailAttachmentContext : AttachmentBase {

        #region Private Fields

        private readonly RelayCommand _saveCommand;
        private readonly RelayCommand _openCommand;
        private readonly MailAttachment _attachment;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        internal MailAttachmentContext(MailAttachment attachment) {
            _attachment = attachment;

            _saveCommand = new RelayCommand(OnSave);
            _openCommand = new RelayCommand(OnOpen);
        }

        #endregion

        #region Properties

        public ICommand OpenCommand {
            get { return _openCommand; }
        }

        public ICommand SaveCommand {
            get { return _saveCommand; }
        }

        #endregion

        internal Task OpenAsync() {
            var name = _attachment.Filename;

            return Task.Run(() => {
                var tempPath = Path.GetTempPath();
                var a = 1;

                while (File.Exists(tempPath + name)) {
                    name = name.Insert(name.LastIndexOf(".", StringComparison.Ordinal) - 1,
                        string.Format("{0}", a));
                    a++;
                }

                var fullname = Path.Combine(tempPath, name);
                File.WriteAllBytes(fullname, Bytes);

                var process = new Process { StartInfo = new ProcessStartInfo(fullname) };
                process.Exited += (sender, e) => {
                    try {
                        File.Delete(fullname);
                    } catch (IOException ex) {
                        Logger.ErrorException(ex.Message, ex);
                    }
                };
                process.Start();
            });
        }

        private async void OnOpen(object obj) {
            Logger.Enter();

            try {
                await OpenAsync();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                Logger.Exit();
            }
        }

        private async void OnSave(object obj) {
            Logger.Enter();

            try {
                var name = _attachment.Filename;
                var extension = name.Split('.').LastOrDefault() ?? string.Empty;

                var dialog = new SaveFileDialog {
                    FileName = name,
                    DefaultExt = extension,
                    Filter = string.Format("{0} (*.*)|*.*", Resources.AllFiles)
                };

                var result = dialog.ShowDialog();
                if (!result.HasValue || !result.Value) {
                    return;
                }

                await Task.Run(() => {
                    try {
                        var bytes = Bytes;
                        File.WriteAllBytes(dialog.FileName, bytes);
                    } catch (Exception ex) {
                        Logger.ErrorException(ex.Message, ex);
                    }
                });

            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                Logger.Exit();
            }
        }

        #region Class Override

        public override string Name {
            get { return _attachment.Filename; }
        }

        public override byte[] GetBytes() {
            Application.Current.AssertBackgroundThread();

            using (var context = new DatabaseContext()) {
                var mime = context.MailMessages.Where(x => x.Id == _attachment.MessageId).Select(x => x.Mime).FirstOrDefault();
                if (mime == null) {
                    return new byte[0];
                }

                var reader = new MailMessageReader(mime);
                var part = reader.FindAllAttachments().FirstOrDefault(x => x.ContentId == _attachment.ContentId);
                return part == null ? new byte[0] : part.Body;
            }
        }

        #endregion
    }
}
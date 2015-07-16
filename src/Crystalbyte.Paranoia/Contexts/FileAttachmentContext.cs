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
using System.Windows.Input;
using Crystalbyte.Paranoia.UI.Commands;
using NLog;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class FileAttachmentContext : AttachmentBase {

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
                Logger.ErrorException(ex.Message, ex);
            }
        }

        #endregion

        #region Properties

        public string FullName {
            get { return _info.FullName; }
        }

        public ICommand OpenCommand {
            get { return _openCommand; }
        }

        #endregion

        #region Class Overrides

        public override string Name {
            get { return _info.Name; }
        }

        public override byte[] GetBytes() {
            return Bytes;
        }

        #endregion
    }
}
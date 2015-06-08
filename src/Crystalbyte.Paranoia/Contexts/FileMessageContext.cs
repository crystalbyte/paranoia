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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class FileMessageContext : NotificationObject, IMailMessage {
        private readonly FileSystemInfo _file;

        public FileMessageContext(FileSystemInfo file) {
            _file = file;
        }

        public string FullName {
            get { return _file.FullName; }
        }

        internal Task LoadAsync() {
            throw new NotImplementedException();
        }

        #region Implementation of IMailMessage

        public string Subject { get; private set; }

        public MailAddressContext From { get; private set; }

        public IReadOnlyCollection<MailAddressContext> To { get; private set; }

        public IReadOnlyCollection<MailAddressContext> Cc { get; private set; }

        public IReadOnlyCollection<MailAttachmentContext> Attachments { get; private set; }

        public DateTime Date { get; private set; }

        public bool IsDownloading { get; set; }
        public double Progress { get; set; }

        #endregion
    }
}
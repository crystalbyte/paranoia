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

using System.Collections.Generic;
using System.Linq;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class NotificationWindowContext : NotificationObject {
        private readonly MailMessageContext _message;

        public NotificationWindowContext(ICollection<MailMessageContext> messages) {
            if (messages.Count > 0) {
                _message = messages.First();
            }
        }

        public string FromAddress {
            get { return _message.FromAddress; }
        }

        public string FromName {
            get { return _message.FromName; }
        }

        public string Subject {
            get { return _message.Subject; }
        }
    }
}
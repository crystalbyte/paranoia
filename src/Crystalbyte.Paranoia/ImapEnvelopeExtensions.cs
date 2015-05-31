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
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;

#endregion

namespace Crystalbyte.Paranoia {
    internal static class ImapEnvelopeExtensions {
        public static MailMessage ToMailMessage(this ImapEnvelope envelope) {
            var message = new MailMessage {
                Date = envelope.InternalDate.HasValue
                    ? envelope.InternalDate.Value
                    : DateTime.Now,
                Subject = envelope.Subject,
                Size = envelope.Size,
                Uid = envelope.Uid,
                MessageId = envelope.MessageId,
            };

            message.Flags.AddRange(envelope.Flags.Select(x => new MailMessageFlag { Value = x }));

            var contacts = envelope.Cc.Select(x => new MailAddress {
                Address = x.Address,
                Name = x.DisplayName,
                Role = AddressRole.Cc
            }).Concat(envelope.To.Select(x => new MailAddress {
                Address = x.Address,
                Name = x.DisplayName,
                Role = AddressRole.To
            }).Concat(envelope.From.Select(x => new MailAddress {
                Address = x.Address,
                Name = x.DisplayName,
                Role = AddressRole.From
            })));

            message.Addresses.AddRange(contacts);
            return message;
        }
    }
}
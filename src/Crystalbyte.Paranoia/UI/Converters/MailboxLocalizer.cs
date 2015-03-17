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
using System.Globalization;
using System.Windows.Data;
using Crystalbyte.Paranoia.Properties;

#endregion

namespace Crystalbyte.Paranoia.UI.Converters {
    [ValueConversion(typeof (MailboxContext), typeof (string))]
    public sealed class MailboxLocalizer : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var context = (MailboxContext) value;
            if (context.IsTrash) {
                return Resources.TrashMailbox;
            }
            if (context.IsDraft) {
                return Resources.DraftMailbox;
            }
            if (context.IsSent) {
                return Resources.SentMailbox;
            }
            if (context.IsJunk) {
                return Resources.JunkMailbox;
            }
            if (context.IsInbox) {
                return Resources.InboxMailbox;
            }

            return context.LocalName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}
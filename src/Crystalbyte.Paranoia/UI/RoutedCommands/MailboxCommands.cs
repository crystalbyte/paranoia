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

using System.Windows.Input;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public static class MailboxCommands {
        public static RoutedCommand SelectRole = new RoutedCommand("SelectRole", typeof (MailboxCommands));
        public static RoutedCommand Browse = new RoutedCommand("Browse", typeof (MailboxCommands));
        public static RoutedCommand Create = new RoutedCommand("Create", typeof (MailboxCommands));
        public static RoutedCommand Delete = new RoutedCommand("Delete", typeof (MailboxCommands));
        public static RoutedCommand Sync = new RoutedCommand("Sync", typeof (MailboxCommands));
    }
}
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
    public static class MessageCommands {
        public static RoutedCommand Compose = new RoutedCommand("Compose", typeof (MessageCommands));
        public static RoutedCommand Reply = new RoutedCommand("Reply", typeof (MessageCommands));
        public static RoutedCommand ReplyAll = new RoutedCommand("ReplyAll", typeof (MessageCommands));
        public static RoutedCommand Forward = new RoutedCommand("Forward", typeof (MessageCommands));
        public static RoutedCommand Resume = new RoutedCommand("Resume", typeof (MessageCommands));
        public static RoutedCommand Inspect = new RoutedCommand("Inspect", typeof (MessageCommands));
        public static RoutedCommand QuickSearch = new RoutedCommand("QuickSearch", typeof (MessageCommands));
        public static RoutedCommand CancelSearch = new RoutedCommand("CancelSearch", typeof(MessageCommands));
    }
}
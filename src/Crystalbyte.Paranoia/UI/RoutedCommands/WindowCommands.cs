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
using Crystalbyte.Paranoia.Properties;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public static class WindowCommands {
        public static RoutedUICommand Minimize = new RoutedUICommand(Resources.Minimize, "Minimize",
            typeof (WindowCommands));

        public static RoutedUICommand Maximize = new RoutedUICommand(Resources.Maximize, "Maximize",
            typeof (WindowCommands));

        public static RoutedUICommand RestoreDown = new RoutedUICommand(Resources.RestoreDown, "RestoreDown",
            typeof (WindowCommands));
    }
}
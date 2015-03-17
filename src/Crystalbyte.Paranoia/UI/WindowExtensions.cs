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
using System.Windows;
using System.Windows.Threading;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public static class WindowExtensions {
        public static void MimicOwnership(this Window window, Window owner) {
            window.Owner = owner;
            window.Height = owner.Height > 500 ? owner.Height*0.9 : 500;
            window.Width = owner.Width > 800 ? owner.Width*0.9 : 800;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Loaded += OnWindowLoaded;
            owner.Closed += (sender, e) => window.Close();
        }

        private static void OnWindowLoaded(object sender, RoutedEventArgs e) {
            var window = (Window) sender;
            window.Loaded -= OnWindowLoaded;

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(400)
            };

            timer.Tick += (x, y) => {
                              timer.Stop();
                              window.Owner = null;
                          };

            timer.Start();
        }
    }
}
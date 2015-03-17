#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia.Themes.Dark
// 
// Crystalbyte.Paranoia.Themes.Dark is free software: you can redistribute it and/or modify
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
using System.Composition;
using System.Windows;

#endregion

namespace Crystalbyte.Paranoia.Themes {
    [Export(typeof (Theme))]
    public sealed class DarkTheme : Theme {
        public override string GetName() {
            return "Dark";
        }

        public override ResourceDictionary GetThemeResources() {
            var url = string.Format(Pack.Relative, typeof (DarkTheme).Assembly.FullName, "/Themes.Dark.Resources.xaml");
            return (ResourceDictionary) Application.LoadComponent(new Uri(url, UriKind.Relative));
        }
    }
}
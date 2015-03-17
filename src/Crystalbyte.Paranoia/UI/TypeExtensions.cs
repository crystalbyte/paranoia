﻿#region Copyright Notice & Copying Permission

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

#endregion

namespace Crystalbyte.Paranoia.UI {
    public static class TypeExtensions {
        public static Uri ToPageUri(this Type type) {
            return new Uri(string.Format("/UI/{0}.xaml", type.Name), UriKind.RelativeOrAbsolute);
        }

        public static Uri ToPageUri(this Type type, string arguments) {
            return new Uri(string.Format("/UI/{0}.xaml{1}", type.Name, arguments), UriKind.Relative);
        }
    }
}
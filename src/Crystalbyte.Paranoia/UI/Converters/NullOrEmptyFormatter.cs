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
using System.Text.RegularExpressions;
using System.Windows.Data;

#endregion

namespace Crystalbyte.Paranoia.UI.Converters {
    [ValueConversion(typeof (string), typeof (string))]
    public sealed class NullOrEmptyFormatter : IValueConverter {
        private const string Sub = "\u200B";
        private const string Pattern = "\n|\r";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var p = parameter as string;
            var s = value as string;
            if (s == null) {
                return p;
            }

            if (s.Equals("NIL") || string.IsNullOrEmpty(s)) {
                return string.IsNullOrWhiteSpace(p)
                    ? string.Empty
                    : Regex.Replace(p, Pattern, Sub);
            }

            return Regex.Replace(s, Pattern, Sub);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var s = (string) value;
            return string.IsNullOrEmpty(s) ? s : Regex.Replace(s, Sub, Pattern);
        }
    }
}
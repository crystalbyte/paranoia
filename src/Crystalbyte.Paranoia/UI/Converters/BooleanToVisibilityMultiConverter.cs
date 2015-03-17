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
using System.Linq;
using System.Windows.Data;

#endregion

namespace Crystalbyte.Paranoia.UI.Converters {
    public sealed class BooleanToVisibilityMultiConverter : IMultiValueConverter {
        #region IMultiValueConverter

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            var booleans = values.Select(x => (bool) x);

            var s = parameter as string;
            if (string.IsNullOrEmpty(s) || string.Compare(s, "AND", StringComparison.InvariantCultureIgnoreCase) == 0) {
                return booleans.All(x => x);
            }

            return booleans.Any(x => x);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            return new[] {Binding.DoNothing};
        }

        #endregion
    }
}
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
using System.Reflection;
using System.Windows.Data;

#endregion

namespace Crystalbyte.Paranoia.UI.Converters {
    public sealed class SortPropertyFormatter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var property = (SortProperty) value;

            var attributes =
                typeof (SortProperty).GetField(property.ToString())
                    .GetCustomAttributes(typeof (DescriptionAttribute), false);

            if (attributes.Length <= 0)
                return property;

            var description = (DescriptionAttribute) attributes[0];
            return description.Type.GetProperty(description.Name)
                .GetValue(null, BindingFlags.Static, null, null, CultureInfo.CurrentCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}
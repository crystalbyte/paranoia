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
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Crystalbyte.Paranoia.Properties;

#endregion

namespace Crystalbyte.Paranoia {
    /// <summary>
    ///     Provides support for extracting property information based on a property expression.
    /// </summary>
    public static class PropertySupport {
        /// <summary>
        ///     Extracts the property name from a property expression.
        /// </summary>
        /// <typeparam name="T"> The object type containing the property specified in the expression. </typeparam>
        /// <param name="propertyExpression"> The property expression (e.g. p => p.PropertyName) </param>
        /// <returns> The name of the property. </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if the
        ///     <paramref name="propertyExpression" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the expression is:
        ///     <br />
        ///     Not a
        ///     <see cref="MemberExpression" />
        ///     <br />
        ///     The
        ///     <see cref="MemberExpression" />
        ///     does not represent a property.
        ///     <br />
        ///     Or, the property is static.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"),
         SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static string ExtractPropertyName<T>(Expression<Func<T>> propertyExpression) {
            if (propertyExpression == null) {
                throw new ArgumentNullException("propertyExpression");
            }

            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null) {
                throw new ArgumentException(Resources.NotMemberAccessExpression, "propertyExpression");
            }

            var property = memberExpression.Member as PropertyInfo;
            if (property == null) {
                throw new ArgumentException(Resources.ExpressionNotProperty, "propertyExpression");
            }

            return memberExpression.Member.Name;
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"),
         SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static string ExtractPropertyName<T, TReturn>(Expression<Func<T, TReturn>> propertyExpression) {
            if (propertyExpression == null) {
                throw new ArgumentNullException("propertyExpression");
            }

            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null) {
                throw new ArgumentException(Resources.NotMemberAccessExpression, "propertyExpression");
            }

            var property = memberExpression.Member as PropertyInfo;
            if (property == null) {
                throw new ArgumentException(Resources.ExpressionNotProperty, "propertyExpression");
            }

            return memberExpression.Member.Name;
        }
    }
}
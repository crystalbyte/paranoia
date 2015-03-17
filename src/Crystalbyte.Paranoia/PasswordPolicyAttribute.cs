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

using System.ComponentModel.DataAnnotations;
using System.Linq;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class PasswordPolicyAttribute : ValidationAttribute {
        private const string Symbols = "!,.-_+*#'~°§$%&/()=?";

        public PasswordPolicyAttribute() {
            MinLength = 4;
            EnforceNumerics = true;
            EnforceLowerChars = true;
            EnforceUpperChars = false;
            EnforceSymbols = true;
        }

        public override bool IsValid(object value) {
            var text = value as string;
            if (string.IsNullOrWhiteSpace(text)) {
                return false;
            }
            var result = true;
            if (MinLength > 0) {
                result &= text.Length > MinLength;
            }
            if (EnforceNumerics) {
                result &= text.Any(char.IsNumber);
            }
            if (EnforceUpperChars) {
                result &= text.Any(char.IsUpper);
            }
            if (EnforceLowerChars) {
                result &= text.Any(char.IsLower);
            }
            if (EnforceSymbols) {
                result &= text.Any(x => Symbols.Contains(x));
            }
            return result;
        }

        public int MinLength { get; set; }

        public bool EnforceNumerics { get; set; }

        public bool EnforceSymbols { get; set; }

        public bool EnforceLowerChars { get; set; }

        public bool EnforceUpperChars { get; set; }
    }
}
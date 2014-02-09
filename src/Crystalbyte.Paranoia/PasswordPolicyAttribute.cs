using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    public sealed class PasswordPolicyAttribute : ValidationAttribute {

        private static string Symbols = "!,.-_+*#'~°!§$%&/()=?";
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
                result &= text.Any(x => char.IsNumber(x));
            }
            if (EnforceUpperChars) {
                result &= text.Any(x => char.IsUpper(x));
            }
            if (EnforceLowerChars) {
                result &= text.Any(x => char.IsLower(x));
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

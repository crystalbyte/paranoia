using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Validation {
    public abstract class ValidationAttribute : Attribute {
        public string ErrorMessage { get; set; }
    }
}

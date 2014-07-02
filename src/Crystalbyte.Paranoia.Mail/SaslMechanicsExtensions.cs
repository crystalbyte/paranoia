#region Using directives

using System;
using System.Linq;

#endregion

namespace Crystalbyte.Paranoia.Mail {
    public static class SaslMechanicsExtensions {
        public static SaslMechanics GetSafest(this SaslMechanics mechanics) {
            return Enum.GetValues(typeof (SaslMechanics))
                .Cast<SaslMechanics>()
                .OrderByDescending(x => x)
                .FirstOrDefault(x => mechanics.HasFlag(x));
        }

        public static SaslMechanics GetFastest(this SaslMechanics mechanics) {
            return Enum.GetValues(typeof (SaslMechanics))
                .Cast<SaslMechanics>()
                .OrderBy(x => x)
                .FirstOrDefault(x => mechanics.HasFlag(x));
        }
    }
}
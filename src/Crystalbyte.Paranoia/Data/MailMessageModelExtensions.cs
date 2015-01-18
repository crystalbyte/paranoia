using System;
using System.Linq;

namespace Crystalbyte.Paranoia.Data {
    internal static class MailMessageModelExtensions {
        public static void DropFlag(this MailMessageModel message, string flag) {
            var flags = message.Flags.Split(';').ToList();
            flags.RemoveAll(x => x.Equals(flag, StringComparison.InvariantCultureIgnoreCase));

            message.Flags = string.Join(";", flags);
        }

        public static void WriteFlag(this MailMessageModel message, string flag) {
            var flags = message.Flags.Split(';').ToList();
            flags.Add(flag);

            message.Flags = string.Join(";", flags);
        }

        public static bool HasFlag(this MailMessageModel message, string flag) {
            return message.Flags.ContainsIgnoreCase(flag);
        }
    }
}

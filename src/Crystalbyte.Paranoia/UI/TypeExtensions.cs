using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Crystalbyte.Paranoia.UI {
    public static class TypeExtensions {
        public static Uri ToPageUri(this Type type) {
            return new Uri(string.Format("/UI/Pages/{0}.xaml", type.Name), UriKind.Relative);
        }

        public static Uri ToPageUri(this Type type, string arguments) {
            return new Uri(string.Format("/UI/Pages/{0}.xaml{1}", type.Name, arguments), UriKind.Relative);
        }

        public static Uri ToPageUriAsReply(this Type type, MailMessageContext mail) {
            return new Uri(string.Format("/UI/Pages/{0}.xaml?action=reply&id={1}", type.Name, mail.Id), UriKind.Relative);
        }
    }
}

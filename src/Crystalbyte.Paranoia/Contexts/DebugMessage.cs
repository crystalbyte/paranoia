using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class DebugMessage : NotificationObject {

        public string Subject {
            get { return "This is a plain text subject."; }
        }

        public string Markup {
            get { return string.Format("<html>This is an html context.</html>"); }
        }
    }
}

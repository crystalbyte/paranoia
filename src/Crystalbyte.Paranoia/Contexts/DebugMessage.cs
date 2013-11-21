using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class DebugMessage : NotificationObject {

        private static readonly string Message;

        static DebugMessage() {
            Message = File.ReadAllText("./Debug/debug.mail.html");
        }

        public DebugMessage(int threadId) {
            ThreadId = string.Format("Thread-{0}", threadId);
        }

        public string ThreadId { get; private set; }

        public string Subject {
            get { return "Appcelerator Newsletter"; }
        }

        public string Markup {
            get { return Message; }
        }
    }
}

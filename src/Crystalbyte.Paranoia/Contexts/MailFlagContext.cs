using Crystalbyte.Paranoia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class MailFlagContext {
        private MailContext _mail;
        private readonly MailFlag _flag;

        public MailFlagContext(MailContext mail, MailFlag flag) {
            _mail = mail;
            _flag = flag;
        }

        public string Name {
            get { return _flag.Name; }
        }

        public bool IsSystemFlag {
            get { return Name.StartsWith(@"\"); }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Contexts {
    [Export, Shared]
    public sealed class LogContext {

        public static LogContext Current {
            get { return App.AppContext.ErrorLogContext; }
        }

        public void PushError(Exception ex) {
            
        }
    }
}

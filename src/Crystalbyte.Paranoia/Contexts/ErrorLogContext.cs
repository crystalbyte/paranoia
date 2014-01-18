using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Contexts {
    [Export, Shared]
    public sealed class ErrorLogContext {

        public static ErrorLogContext Current {
            get { return App.AppContext.ErrorLogContext; }
        }

        public void PushError(Exception ex) {
            
        }
    }
}

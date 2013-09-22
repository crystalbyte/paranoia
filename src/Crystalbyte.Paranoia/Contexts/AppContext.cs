using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.Contexts {
    [Export, Shared]
    public sealed class AppContext {

        public AppContext() {
                        
        }
        public ICommand ConnectCommand { get; set; }
    }
}

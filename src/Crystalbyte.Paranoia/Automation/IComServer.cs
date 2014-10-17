using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Automation {
    public interface IComServer {
        void IncrementObjectCount();
        void DecrementObjectCount();
        void IncrementServerLock();
        void DecrementServerLock();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    public static class QuickAccessRegistry {

        private static readonly List<IQuickAccessConform> Commands 
            = new List<IQuickAccessConform>();
        
        public static void Register(IQuickAccessConform command) {
            Commands.Add(command);
        }

        public static IQuickAccessConform Find(object key) {
            return Commands.FirstOrDefault(x => x.Key == key);
        }
    }
}

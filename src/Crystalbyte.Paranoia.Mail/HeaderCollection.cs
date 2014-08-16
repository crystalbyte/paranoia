using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Mail {
    public sealed class HeaderCollection : List<KeyValuePair<string, string>> {

        public bool ContainsKey(string key) {
            return this.Any(x => x.Key == key);
        }

        public string this[string key] {
            get {
                return this.First(x => x.Key == key).Value;       
            }
        }
    }
}

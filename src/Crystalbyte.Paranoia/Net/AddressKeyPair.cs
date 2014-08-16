using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Net {
    public sealed class AddressKeyPair {

        [JsonProperty("email")]
        public string Address { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }
    }
}

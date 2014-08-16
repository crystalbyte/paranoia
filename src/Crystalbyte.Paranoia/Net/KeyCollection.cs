using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Net {
    public sealed class KeyCollection {

        [JsonProperty("keys")]
        public List<string> Keys { get; set; }
    }
}


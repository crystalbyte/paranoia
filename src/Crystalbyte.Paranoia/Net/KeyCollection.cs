#region Using directives

using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace Crystalbyte.Paranoia.Net {
    public sealed class KeyCollection {
        [JsonProperty("keys")]
        public List<string> Keys { get; set; }
    }
}
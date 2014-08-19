#region Using directives

using Newtonsoft.Json;

#endregion

namespace Crystalbyte.Paranoia.Net {
    public sealed class AddressKeyPair {
        [JsonProperty("email")]
        public string Address { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }
    }
}
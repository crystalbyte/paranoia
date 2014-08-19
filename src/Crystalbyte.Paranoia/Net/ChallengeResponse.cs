#region Using directives

using Newtonsoft.Json;

#endregion

namespace Crystalbyte.Paranoia.Net {
    public sealed class ChallengeResponse {
        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Crystalbyte.Paranoia {
    public struct TextRange {
        [JsonProperty("start")]
        public int Start { get; set; }
        [JsonProperty("end")]
        public int End { get; set; }

        public static TextRange FromPosition(int position) {
            return new TextRange { Start = position, End = position };
        }

        public bool IsPosition {
            get { return Start == End; }
        }
    }
}

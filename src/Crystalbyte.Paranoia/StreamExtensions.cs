using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    public static class StreamExtensions {
        public static async Task<string> ToUtf8StringAsync(this Stream stream) {
            if (stream.CanSeek) {
                stream.Seek(0, SeekOrigin.Begin);
            }

            using (var reader = new StreamReader(stream)) {
                return await reader.ReadToEndAsync();
            }
        }
    }
}

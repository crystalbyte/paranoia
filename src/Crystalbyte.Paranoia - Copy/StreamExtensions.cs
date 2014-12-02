#region Using directives

using System.IO;
using System.Threading.Tasks;

#endregion

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

        public static string ToUtf8String(this Stream stream) {
            if (stream.CanSeek) {
                stream.Seek(0, SeekOrigin.Begin);
            }

            using (var reader = new StreamReader(stream)) {
                return reader.ReadToEnd();
            }
        }
    }
}
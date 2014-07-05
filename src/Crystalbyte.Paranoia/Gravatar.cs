using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Crystalbyte.Paranoia {
    internal static class Gravatar {
        public static string CreateImageUrl(string address) {
            if (string.IsNullOrWhiteSpace(address)) {
                return address;
            }
            using (var md5 = MD5.Create()) {
                var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(address.Trim()));
                using (var writer = new StringWriter()) {
                    foreach (var b in bytes) {
                        writer.Write(b.ToString("x2"));
                    }
                    return string.Format("http://www.gravatar.com/avatar/{0}?s=200&d=mm", writer);
                }
            }
        }
    }
}

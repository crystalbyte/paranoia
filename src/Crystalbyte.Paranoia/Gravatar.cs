#region Using directives

using System.IO;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace Crystalbyte.Paranoia {
    internal static class Gravatar {
        public static string CreateImageUrl(string address, int size = 200) {
            if (string.IsNullOrWhiteSpace(address)) {
                address = "we.the.people@anonymous.com";
            }
            using (var md5 = MD5.Create()) {
                var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(address.Trim()));
                using (var writer = new StringWriter()) {
                    foreach (var b in bytes) {
                        writer.Write(b.ToString("x2"));
                    }
                    return string.Format("https://www.gravatar.com/avatar/{0}?s={1}&d=mm", writer, size);
                }
            }
        }
    }
}
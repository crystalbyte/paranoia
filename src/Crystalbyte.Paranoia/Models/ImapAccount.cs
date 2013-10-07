namespace Crystalbyte.Paranoia.Models {
    public sealed class ImapAccount {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public short Port { get; set; }
    }
}

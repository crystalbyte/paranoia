namespace Crystalbyte.Paranoia.Messaging {
    /// <summary>
    ///   This class enlists all supported content encoding mechanisms.
    ///   http://tools.ietf.org/html/rfc2045#section-6.1
    /// </summary>
    public static class ContentTransferEncodings {
        public const string None = "none";
        public const string QuotedPrintable = "quoted-printable";
        public const string Base64 = "base64";
    }
}
namespace Crystalbyte.Paranoia.Messaging {
    public enum SecurityPolicy : byte {
        /// <summary>
        ///   No security encryption, we strictly discourage you from using this.
        /// </summary>
        None,

        /// <summary>
        ///   Security encryption is mandatory, current operation can't continue without (SSL).
        /// </summary>
        Implicit,

        /// ///
        /// <summary>
        ///   Security encryption will be used when available, if not the connection will commence without (TLS).
        /// </summary>
        Explicit
    }
}
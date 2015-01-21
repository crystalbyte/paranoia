namespace Crystalbyte.Paranoia.UI {
    public sealed class EditorContentLoadedEventArgs {
        internal EditorContentLoadedEventArgs(string html) {
            Html = html;
        }
        public string Html { get; private set; }
    }
}

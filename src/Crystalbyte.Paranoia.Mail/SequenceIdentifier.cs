namespace Crystalbyte.Paranoia.Mail {
    internal static class SequenceIdentifier {
        private static long _current;

        public static string CreateNext() {
            return string.Format("p0{0}", _current++);
        }
    }
}
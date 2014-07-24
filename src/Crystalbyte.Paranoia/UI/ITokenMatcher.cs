namespace Crystalbyte.Paranoia.UI {
    public interface ITokenMatcher {
        bool TryMatch(string value, out string match);
    }
}

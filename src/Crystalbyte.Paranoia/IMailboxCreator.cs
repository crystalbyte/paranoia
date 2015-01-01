using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    public interface IMailboxCreator {
        Task CreateMailboxAsync(string name);
        bool CanHaveChildren { get; }
        bool CheckForValidName(string name);
    }
}

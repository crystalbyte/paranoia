using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    public interface IBlockable {
        bool IsExternalContentAllowed { get; }

        Task BlockAsync();

        Task UnblockAsync();
    }
}

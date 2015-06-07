using System;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    public interface IBlockable {
        event EventHandler IsExternalContentAllowedChanged;

        bool IsExternalContentAllowed { get; }

        Task BlockAsync();

        Task UnblockAsync();
    }
}

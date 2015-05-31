using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;

namespace Crystalbyte.Paranoia {
    public interface IAuthenticatable {
        Authenticity Authenticity { get; }
        Task ConfirmAsync();
        Task RejectAsync();
    }
}

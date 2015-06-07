using System;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;

namespace Crystalbyte.Paranoia {
    public interface IAuthenticatable {

        event EventHandler AuthenticityChanged;
        Authenticity Authenticity { get; }
        Task ConfirmAsync();
        Task RejectAsync();
    }
}

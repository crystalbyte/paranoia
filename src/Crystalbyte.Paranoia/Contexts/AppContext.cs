using System.Composition;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;

namespace Crystalbyte.Paranoia {

    [Export, Shared]
    public sealed class AppContext {

        public UserContext User { get; set; }

        #region Import Directives

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            User = new UserContext { 
                Name = "Alexander Wieser",
                MailAddress = "alexander.wieser@crystalbyte.de"
            };
        }

        #endregion
    }
}

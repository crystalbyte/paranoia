#region Using directives

using Awesomium.Core;
using Awesomium.Core.Data;
using Awesomium.Windows.Data;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public sealed class ParanoiaDataSourceProvider : DataSourceProvider {
        protected override DataSource GetDataSource() {
            return new ParanoiaDataSource();
        }
    }
}
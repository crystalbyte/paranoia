#region Using directives

using Awesomium.Core.Data;
using Awesomium.Windows.Data;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class ParanoiaDataSourceProvider : DataSourceProvider {
        protected override DataSource GetDataSource() {
            return new ParanoiaDataSource();
        }
    }
}
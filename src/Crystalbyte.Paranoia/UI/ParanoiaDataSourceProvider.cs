using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Awesomium.Core.Data;
using Awesomium.Windows.Data;

namespace Crystalbyte.Paranoia.UI {
    public sealed class ParanoiaDataSourceProvider : DataSourceProvider {
        protected override DataSource GetDataSource() {
            return new ParanoiaDataSource();
        }
    }
}

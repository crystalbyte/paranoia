using System.Collections.Generic;
using System.Windows;

namespace Crystalbyte.Paranoia.Themes {
    public abstract class Theme {
        public abstract string GetName();

        public abstract IEnumerable<ResourceDictionary> GetThemeResources();
    }
}

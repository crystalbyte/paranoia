using System.Collections.Generic;
using System.Windows;

namespace Crystalbyte.Paranoia.Themes {
    public abstract class Theme {

        public string Name {
            get { return GetName(); }
        }

        public abstract string GetName();

        public abstract ResourceDictionary GetThemeResources();
    }
}

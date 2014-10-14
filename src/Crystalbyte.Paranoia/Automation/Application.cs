using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Crystalbyte.Paranoia.Automation {

    public sealed class Application : IApplication {

        #region Implementation of IApplication

        public void OpenFile(string path) {
            MessageBox.Show("Automation: OpenFile");
        }

        #endregion
    }
}

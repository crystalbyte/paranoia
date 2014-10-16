using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Crystalbyte.Paranoia.Automation {

    [Guid(ClassId)]
    [ComVisible(true)]
    public sealed class Application : ComObject, IApplication {

        #region COM Registration

        public const string ClassId = "AA1CC4EE-2EB6-4521-940B-5B4C56C46CB0";
        public const string InterfaceId = "D01DE446-E6AD-4BDA-B768-4AB4579FE909";

        #endregion

        #region Construction

        public Application(IComServer server) 
            : base(server) { }

        #endregion

        #region Implementation of IApplication

        public void OpenFile(string path) {
            SystemSounds.Beep.Play();
        }

        #endregion
    }
}

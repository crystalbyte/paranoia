#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia
// 
// Crystalbyte.Paranoia is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License.
// 
// Foobar is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Using Directives

using System.IO;
using System.Runtime.InteropServices;
using WinApp = System.Windows.Application;

#endregion

namespace Crystalbyte.Paranoia.Automation {
    [ComVisible(true)]
    [ProgId(ProgId), Guid(ClassId)]
    [ClassInterface(ClassInterfaceType.None)]
    public sealed class Application : ComObject, IApplication {
        #region COM Registration

        internal const string ProgId = "Paranoia.Application.1";
        internal const string ClassId = "AA1CC4EE-2EB6-4521-940B-5B4C56C46CB0";
        internal const string InterfaceId = "D01DE446-E6AD-4BDA-B768-4AB4579FE909";

        #endregion

        #region Construction

        public Application() {
            // COM demands parameterless constructor.    
        }

        public Application(IComServer server)
            : base(server) {}

        #endregion

        #region Implementation of IApplication

        public void OpenFile(string path) {
            WinApp.Current.Dispatcher.Invoke(
                () => App.Context.InspectMessage(new FileInfo(path)));
        }

        #endregion
    }
}
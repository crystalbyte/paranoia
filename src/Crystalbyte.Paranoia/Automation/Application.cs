﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using dotless.Core.Parser.Tree;

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
            : base(server) { }

        #endregion

        #region Implementation of IApplication

        public void OpenFile(string path) {
            System.Windows.Application.Current.Dispatcher.Invoke(
                () => App.Context.InspectMessage(new FileInfo(path)));
        }

        #endregion
    }
}

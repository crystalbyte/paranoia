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

using System;
using System.Runtime.InteropServices;

#endregion

namespace Crystalbyte.Paranoia.Automation {
    /// <summary>
    ///     This class serves as the class factory for the Application object.
    ///     Microsoft: http://support.microsoft.com/kb/977996
    ///     CodeProject: http://www.codeproject.com/Articles/12579/Building-COM-Servers-in-NET
    /// </summary>
    public sealed class ApplicationClassFactory : IClassFactory {

        #region Private Fields

        private readonly IComServer _server;

        #endregion

        #region Construction

        public ApplicationClassFactory() {
            // COM demands parameterless constructor.
        }

        public ApplicationClassFactory(IComServer server) {
            _server = server;
        }

        #endregion

        #region Implementation of IClassFactory

        public int CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject) {
            if (pUnkOuter != IntPtr.Zero) {
                // The pUnkOuter parameter was non-NULL and the object does not support aggregation.
                Marshal.ThrowExceptionForHR(E_NOAGGREGATION);
            }

            if (riid != Marshal.GenerateGuidForType(typeof (IApplication))
                && riid != IID_IDispatch && riid != IID_IUnknown) {
                Marshal.ThrowExceptionForHR(E_NOINTERFACE);
            }

            // Create the instance of the .NET object.
            ppvObject = Marshal.GetComInterfaceForObject(new Application(_server), typeof (IApplication));
            return S_OK;
        }

        public int LockServer(bool fLock) {
            if (fLock) {
                _server.IncrementServerLock();
            }
            else {
                _server.DecrementServerLock();
            }
            return S_OK;
        }

        #endregion

        #region Native Support

        // ReSharper disable InconsistentNaming
        public static Guid IID_IUnknown = new Guid("{00000000-0000-0000-C000-000000000046}");
        public static Guid IID_IDispatch = new Guid("{00020400-0000-0000-C000-000000000046}");

        private const int S_OK = 0;
        private const int E_NOINTERFACE = unchecked((int) 0x80004002);
        private const int E_NOAGGREGATION = unchecked((int) 0x80040110);
        // ReSharper restore InconsistentNaming

        #endregion
    }
}
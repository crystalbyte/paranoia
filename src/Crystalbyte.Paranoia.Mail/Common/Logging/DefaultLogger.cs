#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia.Mail
// 
// Crystalbyte.Paranoia.Mail is free software: you can redistribute it and/or modify
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

#endregion

namespace Crystalbyte.Paranoia.Mail.Common.Logging {
    /// <summary>
    ///     This is the log that all logging will go trough.
    /// </summary>
    public static class DefaultLogger {
        /// <summary>
        ///     This is the logger used by all logging methods in the assembly.<br />
        ///     You can override this if you want, to move logging to one of your own
        ///     logging implementations.<br />
        ///     <br />
        ///     By default a <see cref="DiagnosticsLogger" /> is used.
        /// </summary>
        public static ILog Log { get; private set; }

        static DefaultLogger() {
            Log = new DiagnosticsLogger();
        }

        /// <summary>
        ///     Changes the default logging to log to a new logger
        /// </summary>
        /// <param name="newLogger"> The new logger to use to send log messages to </param>
        /// <exception cref="ArgumentNullException">
        ///     Never set this to
        ///     <see langword="null" />
        ///     .
        ///     <br />
        ///     Instead you should implement a NullLogger which just does nothing.
        /// </exception>
        public static void SetLog(ILog newLogger) {
            if (newLogger == null)
                throw new ArgumentNullException("newLogger");
            Log = newLogger;
        }
    }
}
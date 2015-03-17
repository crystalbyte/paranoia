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

namespace Crystalbyte.Paranoia.Mail.Common.Logging {
    /// <summary>
    ///     Defines a logger for managing system logging output
    /// </summary>
    public interface ILog {
        /// <summary>
        ///     Logs an error message to the logs
        /// </summary>
        /// <param name="message"> This is the error message to log </param>
        void LogError(string message);

        /// <summary>
        ///     Logs a debug message to the logs
        /// </summary>
        /// <param name="message"> This is the debug message to log </param>
        void LogDebug(string message);
    }
}
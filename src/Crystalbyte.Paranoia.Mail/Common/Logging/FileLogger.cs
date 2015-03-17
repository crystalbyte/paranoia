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
using System.IO;

#endregion

namespace Crystalbyte.Paranoia.Mail.Common.Logging {
    /// <summary>
    ///     This logging object writes application error and debug output to a text file.
    /// </summary>
    public class FileLogger : ILog {
        #region File Logging

        /// <summary>
        ///     Lock object to prevent thread interactions
        /// </summary>
        private static readonly object LogLock;

        /// <summary>
        ///     Static constructor
        /// </summary>
        static FileLogger() {
            // Default log file is defined here
            LogFile = new FileInfo("OpenPOP.log");
            Enabled = true;
            Verbose = false;
            LogLock = new object();
        }

        /// <summary>
        ///     Turns the logging on and off.
        /// </summary>
        public static bool Enabled { get; set; }

        /// <summary>
        ///     Enables or disables the output of Debug level log messages
        /// </summary>
        public static bool Verbose { get; set; }

        /// <summary>
        ///     The file to which log messages will be written
        /// </summary>
        /// <remarks>
        ///     This property defaults to OpenPOP.log.
        /// </remarks>
        public static FileInfo LogFile { get; set; }

        /// <summary>
        ///     Write a message to the log file
        /// </summary>
        /// <param name="text"> The error text to log </param>
        private static void LogToFile(string text) {
            if (text == null)
                throw new ArgumentNullException("text");

            // We want to open the file and append some text to it
            lock (LogLock) {
                using (var sw = LogFile.AppendText()) {
                    sw.WriteLine(DateTime.Now + " " + text);
                    sw.Flush();
                }
            }
        }

        #endregion

        #region ILog Implementation

        /// <summary>
        ///     Logs an error message to the logs
        /// </summary>
        /// <param name="message"> This is the error message to log </param>
        public void LogError(string message) {
            if (Enabled)
                LogToFile(message);
        }

        /// <summary>
        ///     Logs a debug message to the logs
        /// </summary>
        /// <param name="message"> This is the debug message to log </param>
        public void LogDebug(string message) {
            if (Enabled && Verbose)
                LogToFile("DEBUG: " + message);
        }

        #endregion
    }
}
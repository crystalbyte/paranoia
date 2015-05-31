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
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics;
using System.Net.Mail;
using Crystalbyte.Paranoia.Data.SQLite;
using NLog;

#endregion

namespace Crystalbyte.Paranoia.Data {
    internal sealed class DatabaseContext : DbContext {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public DatabaseContext() {
            Logger.Enter();
            Database.SetInitializer(new DatabaseInitializer<DatabaseContext>());
            TimeCreated = Environment.TickCount & Int32.MaxValue;
        }

        #endregion

        #region Properties

        public long TimeCreated { get; set; }

        #endregion

        #region Database Properties

        public DbSet<KeyPair> KeyPairs { get; set; }
        public DbSet<Mailbox> Mailboxes { get; set; }
        public DbSet<MailboxFlag> MailboxFlag { get; set; }
        public DbSet<MailAccount> MailAccounts { get; set; }
        public DbSet<MailAddress> MailAddresses { get; set; }
        public DbSet<MailAttachment> MailAttachments { get; set; }
        public DbSet<MailMessageContent> MailContents { get; set; }
        public DbSet<MailContact> MailContacts { get; set; }
        public DbSet<MailMessage> MailMessages { get; set; }
        public DbSet<PublicKey> PublicKeys { get; set; }
        public DbSet<MailMessageFlag> MailMessageFlags { get; set; }

        #endregion

        #region Implementation of IDisposable

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            var t2 = Environment.TickCount & Int32.MaxValue;

            var delta = t2 - TimeCreated;
            const int treshold = 200;
            if (delta > treshold) {
                var message = string.Format("Context lifetime exceeded {0} milliseconds ({1}s).", treshold, delta / 1000.0f);
                Logger.Error(message);
            }

            Logger.Exit();
        }

        #endregion
    }
}
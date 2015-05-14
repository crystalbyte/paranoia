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

using System.Data.Entity;
using Crystalbyte.Paranoia.Data.SQLite;

#endregion

namespace Crystalbyte.Paranoia.Data {
    internal sealed class DatabaseContext : DbContext {
        public DatabaseContext() {
            Database.SetInitializer(new DatabaseInitializer<DatabaseContext>());
        }

        public DbSet<KeyPair> KeyPairs { get; set; }
        public DbSet<Mailbox> Mailboxes { get; set; }
        public DbSet<MailAccount> MailAccounts { get; set; }
        public DbSet<MailData> MailData { get; set; }
        public DbSet<MailContent> MailContents { get; set; }
        public DbSet<MailContact> MailContacts { get; set; }
        public DbSet<MailMessage> MailMessages { get; set; }
        public DbSet<MailComposition> Compositions { get; set; }
        public DbSet<PublicKey> PublicKeys { get; set; }
        public DbSet<MessageFlag> MessageFlags { get; set; }
    }
}
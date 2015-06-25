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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Data.SQLite;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using NLog;

#endregion

namespace Crystalbyte.Paranoia {
    [DebuggerDisplay("Name = {Name}")]
    public sealed class MailboxContext : HierarchyContext, IMessageSource, IMailboxCreator {

        #region Private Fields

        private bool _isEditing;
        private bool _isIdling;
        private int? _notSeenCount;
        private int? _count;
        private bool _isSyncingMessages;
        private bool _isDownloadingMessage;
        private bool _isLoadingMessages;
        private bool _isListingMailboxes;
        private readonly Mailbox _mailbox;
        private readonly MailAccountContext _account;
        private bool _isLoadingMailboxes;
        private bool _isSyncingMailboxes;
        private int _progress;
        private bool _isSyncedInitially;
        private bool _isSelectedSubtly;


        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        internal MailboxContext(MailAccountContext account, Mailbox mailbox) {
            _account = account;
            _mailbox = mailbox;

            IsExpandedChanged += OnIsExpandedChanged;
        }

        #endregion

        public MailAccountContext Account {
            get { return _account; }
        }

        public string Name {
            get { return _mailbox.Name; }
            set {
                if (_mailbox.Name == value) {
                    return;
                }

                _mailbox.Name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        public bool IsEditing {
            get { return _isEditing; }
            set {
                if (_isEditing == value) {
                    return;
                }

                _isEditing = value;
                RaisePropertyChanged(() => IsEditing);
                RaisePropertyChanged(() => IsListed);
            }
        }

        public bool IsSelectedSubtly {
            get { return _isSelectedSubtly; }
            set {
                if (_isSelectedSubtly == value) {
                    return;
                }
                _isSelectedSubtly = value;
                RaisePropertyChanged(() => IsSelectedSubtly);
            }
        }

        public bool HasListedChildren {
            get { return HasChildren && Children.Any(x => x.IsListed); }
        }

        public override bool IsListed {
            get { return IsEditing || IsSubscribed || HasListedChildren; }
        }

        public string LocalName {
            get {
                return string.IsNullOrEmpty(Name)
                    ? string.Empty
                    : Name.Split(new[] { Delimiter },
                        StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            }
        }

        public Int64 Id {
            get { return _mailbox.Id; }
            set {
                if (_mailbox.Id == value) {
                    return;
                }

                _mailbox.Id = value;
                RaisePropertyChanged(() => Id);
            }
        }

        protected override async void OnSelectionChanged() {
            base.OnSelectionChanged();

            Application.Current.AssertUIThread();

            try {
                // Only sync if never synced before.
                if (_isSyncedInitially)
                    return;

                if (!IsSyncingMailboxes) {
                    await SyncMailboxesAsync();
                }

                if (!IsSyncingMessages) {
                    await SyncMessagesAsync();
                }

                _isSyncedInitially = true;
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        internal async Task SyncMailboxesAsync() {
            Logger.Enter();

            Application.Current.AssertUIThread();

            if (IsSyncingMailboxes) {
                throw new InvalidOperationException("IsSyncingChildren");
            }

            IsSyncingMailboxes = true;

            try {
                var id = _account.Id;
                var addedMailboxes = await Task.Run(async () => {
                    var resultSet = new List<Mailbox>();
                    var pattern = string.Format("{0}{1}%", Name, Delimiter);
                    var children = await _account.ListMailboxesAsync(pattern);

                    using (var context = new DatabaseContext()) {
                        foreach (var child in children) {
                            var lc = child;
                            var mailbox = await context.Mailboxes
                                .FirstOrDefaultAsync(
                                    x => x.Name == lc.Fullname);

                            if (mailbox != null) {
                                continue;
                            }

                            var model = new Mailbox {
                                AccountId = id,
                                Name = lc.Fullname,
                                Delimiter =
                                    lc.Delimiter.ToString(
                                        CultureInfo.InvariantCulture)
                            };

                            model.Flags.AddRange(child.Flags.Select(x => new MailboxFlag { Value = x }));
                            context.Mailboxes.Add(model);

                            resultSet.Add(model);
                        }

                        await context.SaveChangesAsync(
                            OptimisticConcurrencyStrategy.ClientWins);
                        return resultSet;
                    }
                });

                var contexts = addedMailboxes.Select(x => new MailboxContext(Account, x));
                Account.NotifyMailboxesAdded(contexts);

            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                IsSyncingMailboxes = false;
                Logger.Exit();
            }
        }

        public bool IsLoadingMailboxes {
            get { return _isLoadingMailboxes; }
            set {
                if (_isLoadingMailboxes == value) {
                    return;
                }
                _isLoadingMailboxes = value;
                RaisePropertyChanged(() => IsLoadingMailboxes);
            }
        }

        public bool IsSyncingMailboxes {
            get { return _isSyncingMailboxes; }
            set {
                if (_isSyncingMailboxes == value) {
                    return;
                }
                _isSyncingMailboxes = value;
                RaisePropertyChanged(() => IsSyncingMailboxes);
            }
        }

        public IEnumerable<MailboxContext> Children {
            get {
                var prefix = string.Format("{0}{1}", Name, Delimiter);
                var mailboxes = _account.Mailboxes
                    .Where(x => x.Name.StartsWith(prefix)).ToArray();

                var m = mailboxes
                    .Where(x => !x.Name.Substring(prefix.Length, x.Name.Length - prefix.Length).Contains(x.Delimiter))
                    .ToArray();

                return m;
            }
        }

        internal string Delimiter {
            get { return _mailbox.Delimiter; }
        }

        public bool IsSubscribedAndSelectable {
            get { return IsSubscribed && IsSelectable; }
        }

        public bool IsSubscribed {
            get { return _mailbox.IsSubscribed; }
            set {
                if (_mailbox.IsSubscribed == value) {
                    return;
                }
                _mailbox.IsSubscribed = value;
                RaisePropertyChanged(() => IsSubscribed);
                RaisePropertyChanged(() => IsSubscribedAndSelectable);
                RaisePropertyChanged(() => IsListed);
            }
        }

        internal bool IsTrash {
            get { return _account.TrashMailboxName.EqualsIgnoreCase(Name); }
        }

        internal bool IsJunk {
            get { return _account.JunkMailboxName.EqualsIgnoreCase(Name); }
        }

        internal bool IsDraft {
            get { return _account.DraftMailboxName.EqualsIgnoreCase(Name); }
        }

        internal bool IsSent {
            get { return _account.SentMailboxName.EqualsIgnoreCase(Name); }
        }

        public bool IsInbox {
            get { return Name.EqualsIgnoreCase("inbox"); }
        }

        public bool IsListingMailboxes {
            get { return _isListingMailboxes; }
            set {
                if (_isListingMailboxes == value) {
                    return;
                }
                _isListingMailboxes = value;
                RaisePropertyChanged(() => IsListingMailboxes);
            }
        }

        public int? NotSeenCount {
            get { return _notSeenCount; }
            set {
                if (_notSeenCount == value) {
                    return;
                }
                _notSeenCount = value;
                RaisePropertyChanged(() => NotSeenCount);
            }
        }

        public int? Count {
            get { return _count; }
            set {
                if (_count == value) {
                    return;
                }

                _count = value;
                RaisePropertyChanged(() => Count);
            }
        }

        public bool IsSyncingMessages {
            get { return _isSyncingMessages; }
            set {
                if (_isSyncingMessages == value) {
                    return;
                }
                _isSyncingMessages = value;
                RaisePropertyChanged(() => IsSyncingMessages);
            }
        }

        public bool IsDownloadingMessage {
            get { return _isDownloadingMessage; }
            set {
                if (_isDownloadingMessage == value) {
                    return;
                }
                _isDownloadingMessage = value;
                RaisePropertyChanged(() => IsDownloadingMessage);
            }
        }

        public bool IsSelectable {
            get {
                return !_mailbox.Flags
                    .Any(x => x.Value.EqualsIgnoreCase(MailboxFlags.NoSelect));
            }
        }

        internal async Task DeleteAsync() {
            Logger.Enter();

            Application.Current.AssertUIThread();

            try {
                var deleteFromServer = Task.Run(async () => {
                    using (
                        var connection = new ImapConnection {
                            Security = _account.ImapSecurity
                        }) {
                        using (var auth = await connection.ConnectAsync(
                            _account.ImapHost, _account.ImapPort)) {
                            using (var session = await auth.LoginAsync(
                                _account.ImapUsername, _account.ImapPassword)) {
                                await session.DeleteMailboxAsync(Name);
                            }
                        }
                    }
                });

                var deleteFromStore = Task.Run(async () => {
                    using (var context = new DatabaseContext()) {
                        await context.OpenAsync();
                        await context.EnableForeignKeysAsync();

                        var model = new Mailbox { Id = _mailbox.Id };
                        context.Mailboxes.Remove(model);

                        await context.SaveChangesAsync(
                            OptimisticConcurrencyStrategy.ClientWins);
                    }
                });

                await Task.WhenAll(new[] { deleteFromServer, deleteFromStore });
                _account.NotifyMailboxesRemoved(new[] { this });
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                Logger.Exit();
            }
        }

        public async Task CreateMailboxAsync(string mailbox) {
            Logger.Enter();

            Application.Current.AssertUIThread();

            try {
                var createOnServer = Task.Run(async () => {
                    using (var connection = new ImapConnection { Security = _account.ImapSecurity }) {
                        using (var auth = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                            using (var session = await auth.LoginAsync(_account.ImapUsername, _account.ImapPassword)) {
                                var name = string.Format("{0}{1}{2}", Name, Delimiter, mailbox);
                                await session.CreateMailboxAsync(name);
                            }
                        }
                    }
                });

                var createInStore = Task.Run(async () => {
                    using (var context = new DatabaseContext()) {
                        var model = new Mailbox {
                            AccountId = Account.Id,
                            Delimiter = Delimiter,
                            Name = string.Format("{0}{1}{2}", Name, Delimiter, mailbox),
                        };

                        context.Mailboxes.Add(model);
                        await context.SaveChangesAsync(OptimisticConcurrencyStrategy.ClientWins);
                        return model;
                    }
                });

                Account.NotifyMailboxesAdded(new[] { new MailboxContext(Account, await createInStore) });

                await createOnServer;
                IsExpanded = true;
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                Logger.Exit();
            }
        }

        public bool CanHaveChildren {
            get {
                return !_mailbox.Flags
                    .Any(x => x.Value.EqualsIgnoreCase(MailboxFlags.NoChildren));
            }
        }

        public bool CheckForValidName(string name) {
            return Children.All(x => string.Compare(name, x.LocalName, StringComparison.OrdinalIgnoreCase) != 0);
        }

        private async Task SubscribeAsync() {
            using (var connection = new ImapConnection { Security = _account.ImapSecurity }) {
                using (var auth = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                    using (var session = await auth.LoginAsync(_account.ImapUsername, _account.ImapPassword)) {
                        await session.SubscribeAsync(Name);
                    }
                }
            }

            using (var database = new DatabaseContext()) {
                var mailbox = await database.Mailboxes.FindAsync(_mailbox.Id);
                mailbox.IsSubscribed = true;
                await database.SaveChangesAsync();
            }
        }

        private async Task UnsubscribeAsync() {
            using (var connection = new ImapConnection { Security = _account.ImapSecurity }) {
                using (var auth = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                    using (var session = await auth.LoginAsync(_account.ImapUsername, _account.ImapPassword)) {
                        await session.UnsubscribeAsync(Name);
                    }
                }
            }

            using (var database = new DatabaseContext()) {
                var mailbox = await database.Mailboxes.FindAsync(_mailbox.Id);
                mailbox.IsSubscribed = false;
                await database.SaveChangesAsync();
            }
        }

        internal async Task SyncMessagesAsync() {
            Logger.Enter();

            Application.Current.AssertUIThread();

            IsSyncingMessages = true;

            try {
                var name = _mailbox.Name;
                var getMaxUid = GetMaxUidAsync();
                var getMessageIds = Task.Run(() => {
                    using (var context = new DatabaseContext()) {
                        return context.Set<MailMessage>()
                                .Where(x => x.MailboxId == _mailbox.Id)
                                .Select(x => new { x.Id, x.Uid })
                                .ToDictionaryAsync(x => x.Uid);
                    }
                });

                var getSeenUids = SearchAsync(name, "1:* SEEN");
                var getUnseenUids = SearchAsync(name, "1:* NOT SEEN");
                var fetchEnvelopes = FetchEnvelopesAsync(name, await getMaxUid);

                var messages = (await fetchEnvelopes).ToArray();
                var unseenUids = await getUnseenUids;
                var seenUids = await getSeenUids;

                var uids = new HashSet<long>(unseenUids.Concat(seenUids).Distinct());
                var storedIds = await getMessageIds;
                var obsoleteIds = storedIds.Where(x => !uids.Contains(x.Key)).ToArray();

                if (obsoleteIds.Length > 0) {
                    await DeleteMessagesAsync(obsoleteIds.Select(x => x.Value.Id).ToArray());
                }

                if (messages.Length > 0) {
                    await StoreMessagesAsync(messages);
                }

                await SyncLocalSeenFlagsAsync(seenUids, unseenUids);

                var countNotSeen = CountMessagesAsync();
                var contexts = messages
                    .Select(x => new MailMessageContext(this, x))
                    .ToArray();

                App.Context.NotifyMessagesReceived(contexts);
                await countNotSeen;

            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                IsSyncingMessages = false;
                Logger.Exit();
            }
        }

        private async Task SyncLocalSeenFlagsAsync(HashSet<Int64> seenUids, HashSet<Int64> unseenUids) {
            var seenIds = await Task.Run(async () => {
                using (var context = new DatabaseContext()) {
                    var messages = await context.Set<MailMessage>()
                        .Where(x => x.MailboxId == _mailbox.Id)
                        .Include(x => x.Flags)
                        .Select(x => new { x.Id, x.Uid, x.Flags })
                        .ToArrayAsync();

                    var seen = messages.Where(x => x.Flags.Any(y => y.Value.EqualsIgnoreCase(MailMessageFlags.Seen)))
                        .ToDictionary(x => x.Uid);

                    var unseen = messages.Where(x => x.Flags.All(y => !y.Value.EqualsIgnoreCase(MailMessageFlags.Seen)))
                        .ToDictionary(x => x.Uid);

                    var toBeMarkedAsSeen = unseen.Where(x => seenUids.Contains(x.Key))
                        .Select(x => new { x.Value.Id, x.Value.Flags });

                    var toBeMarkedAsUnseen = seen.Where(x => unseenUids.Contains(x.Key))
                        .Select(x => new { x.Value.Id, x.Value.Flags });

                    var set = context.Set<MailMessageFlag>();
                    var seenFlags = toBeMarkedAsUnseen.Select(x => x.Flags.First(y => y.Value == MailMessageFlags.Seen));
                    set.RemoveRange(seenFlags);
                    set.AddRange(toBeMarkedAsSeen.Select(x => new MailMessageFlag { Value = MailMessageFlags.Seen }));

                    await context.SaveChangesAsync(OptimisticConcurrencyStrategy.ClientWins);

                    return new HashSet<Int64>(seen.Values.Select(x => x.Id));
                }
            });

            App.Context.NotifySeenStatesChanged(seenIds);
        }

        private async void OnIsExpandedChanged(object sender, EventArgs e) {
            try {
                if (!IsExpanded) {
                    return;
                }

                var tasks = Children.Where(x => !x.NotSeenCount.HasValue).Select(x => x.CountMessagesAsync());
                await Task.WhenAll(tasks);

            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private async Task StoreMessagesAsync(ICollection<MailMessage> messages) {
            Logger.Enter();

            // Set the foreign key manually.
            messages.ForEach(x => x.MailboxId = _mailbox.Id);

            try {
                await Task.Run(async () => {
                    using (var context = new DatabaseContext()) {
                        await context.OpenAsync();
                        await context.EnableForeignKeysAsync();

                        using (var transaction = context.Database.Connection.BeginTransaction()) {
                            try {

                                var mailbox = new Mailbox { Id = Id };
                                context.Set<Mailbox>().Attach(mailbox);
                                mailbox.Messages.AddRange(messages);

                                await context.SaveChangesAsync(
                                    OptimisticConcurrencyStrategy.ClientWins);

                                using (var command = context.Database.Connection.CreateCommand()) {
                                    foreach (var message in messages) {
                                        var subjectParam = new SQLiteParameter("@text", message.Subject);
                                        var messageIdParam = new SQLiteParameter("@message_id", message.Id);

                                        var tableAttribute = typeof(MailMessageContent).GetCustomAttribute<TableAttribute>();
                                        var tableName = tableAttribute != null
                                            ? tableAttribute.Name
                                            : typeof(MailMessageContent).Name;

                                        command.CommandText = string.Format("INSERT INTO {0}(text, message_id) VALUES(@text, @message_id);", tableName);
                                        command.Parameters.AddRange(new[] { subjectParam, messageIdParam });
                                        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                                    }
                                }

                                await StoreContactsAsync(context, messages);
                                await context.SaveChangesAsync(
                                    OptimisticConcurrencyStrategy.ClientWins);

                                transaction.Commit();
                            } catch (Exception ex) {
                                Logger.ErrorException(ex.Message, ex);
                                transaction.Rollback();
                            }
                        }
                    }
                });
            } finally {
                Logger.Exit();
            }
        }

        private static async Task StoreContactsAsync(DatabaseContext context, IEnumerable<MailMessage> messages) {
            Logger.Enter();

            Application.Current.AssertBackgroundThread();

            try {
                var contacts = await context.MailContacts.ToDictionaryAsync(x => x.Address.ToLower());

                var mailContacts = messages.SelectMany(x => x.Addresses).ToArray();
                var groups = mailContacts
                    .Where(x => !string.IsNullOrEmpty(x.Address))
                    .GroupBy(x => x.Address.ToLower())
                    .ToArray();

                var t1 = Environment.TickCount & Int32.MaxValue;

                var query = groups
                    .Where(x => !contacts.ContainsKey(x.Key))
                        .Select(group => new MailContact {
                            Address = group.First().Address,
                            Name = group.First().Name.ToLower(CultureInfo.InvariantCulture)
                        });

                var newContacts = query.ToArray();
                context.MailContacts.AddRange(newContacts);

                var t2 = Environment.TickCount & Int32.MaxValue;
                Logger.Debug("Inserted {0} contacts in {1} seconds.", newContacts.Length, (t2 - t1) / 1000.0f);

                await context.SaveChangesAsync(OptimisticConcurrencyStrategy.ClientWins);
            } finally {
                Logger.Exit();
            }
        }

        private Task<MailMessage[]> FetchEnvelopesAsync(string mailboxName, long maxUid) {
            Logger.Enter();

            try {
                return Task.Run(async () => {
                    using (var connection = new ImapConnection { Security = _account.ImapSecurity }) {
                        using (var auth = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                            using (var session = await auth.LoginAsync(_account.ImapUsername,
                                            _account.ImapPassword)) {
                                var mailbox = await session.SelectAsync(mailboxName);

                                var uids = await mailbox.SearchAsync(string.Format("{0}:*", maxUid));

                                mailbox.EnvelopeFetched += OnEnvelopeFetched;
                                var envelopes = await mailbox.FetchEnvelopesAsync(uids);
                                mailbox.EnvelopeFetched -= OnEnvelopeFetched;

                                var duplicate = envelopes.FirstOrDefault(x => x.Uid == maxUid);
                                if (duplicate != null) {
                                    envelopes.Remove(duplicate);
                                }


                                return envelopes.Select(x => x.ToMailMessage()).ToArray();
                            }
                        }
                    }
                });
            } finally {
                Logger.Exit();
            }
        }

        private Task<HashSet<Int64>> SearchAsync(string mailboxName, string pattern) {
            Logger.Enter();

            try {
                return Task.Run(async () => {
                    using (var connection = new ImapConnection {
                        Security = _account.ImapSecurity
                    }) {
                        using (var auth = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                            using (var session = await auth.LoginAsync(_account.ImapUsername,
                                            _account.ImapPassword)) {
                                var mailbox = await session.SelectAsync(mailboxName);
                                return new HashSet<long>(await mailbox.SearchAsync(pattern));
                            }
                        }
                    }
                });
            } finally {
                Logger.Exit();
            }
        }

        private async Task DeleteMessagesAsync(ICollection<Int64> ids) {
            Logger.Enter();

            Application.Current.AssertUIThread();

            var t1 = Environment.TickCount & Int32.MaxValue;

            try {
                await Task.Run(async () => {
                    using (var context = new DatabaseContext()) {
                        await context.OpenAsync();
                        await context.EnableForeignKeysAsync();

                        foreach (var id in ids) {
                            var model = new MailMessage { Id = id };
                            context.Set<MailMessage>().Attach(model);
                            context.Set<MailMessage>().Remove(model);
                        }
                    }
                });

                var t2 = Environment.TickCount & Int32.MaxValue;

                Logger.Debug("Deleted {0} messages from mailbox {1} in {2} seconds.",
                    ids.Count, Name, (t2 - t1) / 1000.0f);
            } finally {
                Logger.Exit();
            }
        }

        /// <summary>
        /// Gets or sets the progress as an integer from 0 to 100.
        /// </summary>
        public int Progress {
            get { return _progress; }
            set {
                if (_progress == value) {
                    return;
                }
                _progress = value;
                RaisePropertyChanged(() => Progress);
            }
        }

        private async void OnEnvelopeFetched(object sender, EnvelopeFetchedEventArgs e) {
            Application.Current.AssertBackgroundThread();

            try {
                await Application.Current.Dispatcher.InvokeAsync(() => {
                    Progress = Convert.ToInt32(e.Progress * 100);
                });
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        internal async Task MarkAsAnsweredAsync(MailMessageContext[] messages) {
            Application.Current.AssertBackgroundThread();

            try {
                messages.ForEach(x => x.IsSeen = false);
                var uids = messages.Select(x => x.Uid).ToArray();

                using (var connection = new ImapConnection { Security = _account.ImapSecurity }) {
                    connection.RemoteCertificateValidationFailed += (sender, e) => e.IsCanceled = false;
                    using (var auth = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                        using (var session = await auth.LoginAsync(_account.ImapUsername, _account.ImapPassword)) {
                            var folder = await session.SelectAsync(Name);
                            await folder.MarkAsAnsweredAsync(uids);
                        }
                    }
                }

                await CountMessagesAsync();
            } catch (Exception ex) {
                messages.ForEach(x => x.IsSeen = true);
                Logger.ErrorException(ex.Message, ex);
            }
        }

        internal async Task MarkAsNotAnsweredAsync(MailMessageContext[] messages) {
            Application.Current.AssertBackgroundThread();

            try {
                messages.ForEach(x => x.IsSeen = false);
                var uids = messages.Select(x => x.Uid).ToArray();

                using (var connection = new ImapConnection { Security = _account.ImapSecurity }) {
                    connection.RemoteCertificateValidationFailed += (sender, e) => e.IsCanceled = false;
                    using (var auth = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                        using (var session = await auth.LoginAsync(_account.ImapUsername, _account.ImapPassword)) {
                            var folder = await session.SelectAsync(Name);
                            await folder.MarkAsNotAnsweredAsync(uids);
                        }
                    }
                }

                await CountMessagesAsync();
            } catch (Exception ex) {
                messages.ForEach(x => x.IsSeen = true);
                Logger.ErrorException(ex.Message, ex);
            }
        }

        internal async Task CountMessagesAsync() {
            Logger.Enter();

            Application.Current.AssertUIThread();

            try {
                var t1 = Environment.TickCount & Int32.MaxValue;

                var counts = await Task.Run(async () => {
                    using (var context = new DatabaseContext()) {
                        var unseen = context.MailMessages
                            .Where(x => x.MailboxId == _mailbox.Id
                                && x.Flags.All(y => y.Value != MailMessageFlags.Seen))
                            .CountAsync();
                        var total = context.MailMessages
                            .Where(x => x.MailboxId == _mailbox.Id)
                            .CountAsync();

                        return new { Unseen = await unseen, Total = await total };
                    }
                });

                NotSeenCount = counts.Unseen;
                Count = counts.Total;

                var t2 = Environment.TickCount & Int32.MaxValue;
                Logger.Debug("Counted {0} messages in mailbox {1} in {2} seconds.", NotSeenCount, Name, (t2 - t1) / 1000.0f);

            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                Logger.Exit();
            }
        }

        private async Task<Int64> GetMaxUidAsync() {
            Logger.Enter();

            try {
                var t1 = Environment.TickCount & Int32.MaxValue;

                var uid = await Task.Run(() => {
                    using (var context = new DatabaseContext()) {
                        return context.MailMessages
                            .Where(x => x.MailboxId == _mailbox.Id)
                            .Select(x => x.Uid)
                            .DefaultIfEmpty(1)
                            .MaxAsync(x => x);
                    }
                });

                var t2 = Environment.TickCount & Int32.MaxValue;
                Logger.Debug("Read {0} max uid from mailbox {1} in {2} seconds.", uid, Name, (t2 - t1) / 1000.0f);

                return uid;
            } finally {
                Logger.Exit();
            }
        }

        public bool IsIdling {
            get { return _isIdling; }
            set {
                if (_isIdling == value) {
                    return;
                }

                _isIdling = value;
                RaisePropertyChanged(() => IsIdling);
            }
        }

        internal async Task IdleAsync() {
            Application.Current.AssertUIThread();

            IsIdling = true;

            try {
                await Task.Run(async () => {
                    Thread.CurrentThread.Name = string.Format("IDLE Thread ({0})", Name);
                    using (var connection = new ImapConnection { Security = _account.ImapSecurity }) {
                        using (
                            var auth =
                                await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)
                            ) {
                            using (
                                var session = await auth.LoginAsync(_account.ImapUsername,
                                            _account.ImapPassword)) {
                                if (!connection.CanIdle) {
                                    Logger.Info(Resources.IdleCommandNotSupported);
                                    return;
                                }
                                var mailbox = await session.SelectAsync(Name);
                                try {
                                    mailbox.ChangeNotificationReceived +=
                                        OnChangeNotificationReceived;
                                    await mailbox.IdleAsync();
                                } finally {
                                    mailbox.ChangeNotificationReceived -=
                                        OnChangeNotificationReceived;
                                }
                            }
                        }
                    }

                });
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                IsIdling = false;
            }
        }

        private async void OnChangeNotificationReceived(object sender, EventArgs e) {
            Application.Current.AssertBackgroundThread();

            try {
                Logger.Info(Resources.IdleChangeNotification);
                await Application.Current.Dispatcher.InvokeAsync(async () => {
                    try {
                        if (IsSyncingMessages) {
                            return;
                        }

                        await SyncMessagesAsync();
                    } catch (Exception ex) {
                        Logger.ErrorException(ex.Message, ex);
                    }
                });
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        public bool HasChildren {
            get { return Children.Any(); }
        }

        public bool IsLoadingMessages {
            get { return _isLoadingMessages; }
            set {
                if (_isLoadingMessages == value) {
                    return;
                }
                _isLoadingMessages = value;
                RaisePropertyChanged(() => IsLoadingMessages);
            }
        }

        /// <summary>
        ///     Moves messages from the trash mailbox back to the inbox.
        /// </summary>
        /// <param name="messages">The messages to move.</param>
        internal async Task RestoreMessagesAsync(MailMessageContext[] messages) {
            Logger.Enter();

            Application.Current.AssertUIThread();

            try {
                if (messages.Length < 1) {
                    return;
                }

                var inbox = messages.First().Mailbox.Account.GetInbox();
                using (var connection = new ImapConnection { Security = _account.ImapSecurity }) {
                    using (var auth = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                        using (var session = await auth.LoginAsync(_account.ImapUsername, _account.ImapPassword)) {
                            var mailbox = await session.SelectAsync(Name);
                            if (IsTrash) {
                                await mailbox.MoveMailsAsync(messages.Select(x => x.Uid).ToArray(), inbox.Name);
                            }
                        }
                    }
                }

                await App.Context.DeleteMessagesAsync(messages);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                Logger.Exit();
            }
        }

        #region Implementation of IMessageSource

        public void BeginQuery() {
            Application.Current.AssertUIThread();
            IsLoadingMessages = true;
        }

        public void FinishQuery() {
            Application.Current.AssertUIThread();
            IsLoadingMessages = false;
        }

        public async Task<IEnumerable<MailMessageContext>> GetMessagesAsync() {
            Application.Current.AssertBackgroundThread();

            MailMessage[] messages = null;
            var t1 = Environment.TickCount & Int32.MaxValue;

            try {
                using (var context = new DatabaseContext()) {
                    await context.OpenAsync();
                    await context.EnableForeignKeysAsync();

                    // We do not use EF's .Include() method for it translates into sub selects which are painfully slow.
                    // Instead we fetch the messages and sub tables manually using INNER JOINS in parallel.
                    var getMessages = context.Set<MailMessage>()
                        .AsNoTracking()
                        .Where(x => x.MailboxId == _mailbox.Id)
                        .Select(x => new {
                            x.Id,
                            x.Uid,
                            x.MailboxId,
                            x.Date,
                            x.Subject,
                            x.Size
                        });

                    var getFlags = context.Set<MailMessageFlag>()
                        .AsNoTracking()
                        .Where(x => x.Message.MailboxId == _mailbox.Id)
                        .Select(x => new { x.MessageId, Flag = x });

                    var getAttachments = context.Set<MailAttachment>()
                        .AsNoTracking()
                        .Where(x => x.Message.MailboxId == _mailbox.Id)
                        .Select(x => new { x.MessageId, Attachment = x });

                    var getAddresses = context.Set<MailAddress>()
                        .AsNoTracking()
                        .Where(x => x.Message.MailboxId == _mailbox.Id)
                        .Select(x => new { x.MessageId, Address = x });

                    var addresses = (await getAddresses.ToArrayAsync())
                        .AsParallel()
                        .GroupBy(x => x.MessageId)
                        .ToDictionary(x => x.Key);

                    var flags = (await getFlags.ToArrayAsync())
                        .AsParallel()
                        .GroupBy(x => x.MessageId)
                        .ToDictionary(x => x.Key);

                    var attachments = (await getAttachments.ToArrayAsync())
                        .AsParallel()
                        .GroupBy(x => x.MessageId)
                        .ToDictionary(x => x.Key);

                    var set = (await getMessages.ToArrayAsync());
                    messages = set.Select(x => new MailMessage {
                        Id = x.Id,
                        Uid = x.Uid,
                        MailboxId = x.MailboxId,
                        Date = x.Date,
                        Subject = x.Subject,
                        Size = x.Size,
                    }).ToArray();

                    foreach (var message in messages) {
                        if (flags.ContainsKey(message.Id)) {
                            message.Flags.AddRange(flags[message.Id].Select(x => x.Flag));
                        }

                        if (attachments.ContainsKey(message.Id)) {
                            message.Attachments.AddRange(attachments[message.Id].Select(x => x.Attachment));
                        }

                        if (addresses.ContainsKey(message.Id)) {
                            message.Addresses.AddRange(addresses[message.Id].Select(x => x.Address));
                        }
                    }

                    if (App.Context.ShowOnlyUnseen) {
                        return messages
                            .Where(x => x.Flags.All(y => y.Value != MailMessageFlags.Seen))
                            .Select(x => new MailMessageContext(this, x));
                    }

                    return messages.Select(x => new MailMessageContext(this, x));
                }
            } finally {
                var t2 = Environment.TickCount & Int32.MaxValue;
                Logger.Debug("Queryied {0} messages in mailbox {1} in {2} seconds.",
                    (messages ?? new MailMessage[0]).Length, Name, (t2 - t1) / 1000.0f);
            }
        }

        #endregion
    }
}
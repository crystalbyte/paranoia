﻿#region Copyright Notice & Copying Permission

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
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Data.SQLite;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI;
using NLog;

#endregion

namespace Crystalbyte.Paranoia {
    [DebuggerDisplay("Name = {Name}")]
    public sealed class MailboxContext : HierarchyContext, IMessageSource, IMailboxCreator {
        #region Private Fields

        private bool _isEditing;
        private bool _isIdling;
        private int _notSeenCount;
        private bool _isSyncingMessages;
        private bool _isDownloadingMessage;
        private bool _isLoadingMessages;
        private bool _isListingMailboxes;
        private readonly Mailbox _mailbox;
        private readonly MailAccountContext _account;
        private bool _isLoadingMailboxes;
        private bool _isSyncingMailboxes;
        private int _totalEnvelopeCount;
        private int _fetchedEnvelopeCount;
        private bool _isSyncedInitially;
        private bool _isSelectedSubtly;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        internal MailboxContext(MailAccountContext account, Mailbox mailbox) {
            _account = account;
            _mailbox = mailbox;
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
                Logger.Error(ex);
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
                                        CultureInfo.InvariantCulture),
                                Flags = string.Join(";", child.Flags)
                            };

                            context.Mailboxes.Add(model);
                            resultSet.Add(model);
                        }

                        await context.SaveChangesAsync(
                            OptimisticConcurrencyStrategy.ClientWins);
                        return resultSet;
                    }
                });

                var contexts = addedMailboxes.Select(x => new MailboxContext(Account, x));
                Account.AddMailboxes(contexts);

            } catch (Exception ex) {
                Logger.Error(ex);
            } finally {
                IsSyncingMailboxes = false;
                Logger.Exit();
            }
        }

        public bool IsLoadingChildren {
            get { return _isLoadingMailboxes; }
            set {
                if (_isLoadingMailboxes == value) {
                    return;
                }
                _isLoadingMailboxes = value;
                RaisePropertyChanged(() => IsLoadingChildren);
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

                if (value) {
                    Task.Run(() => SubscribeAsync());
                } else {
                    Task.Run(() => UnsubscribeAsync());
                }
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

        public int NotSeenCount {
            get { return _notSeenCount; }
            set {
                if (_notSeenCount == value) {
                    return;
                }
                _notSeenCount = value;
                RaisePropertyChanged(() => NotSeenCount);
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
            get { return !_mailbox.Flags.ContainsIgnoreCase(MailboxFlags.NoSelect); }
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
                        await context.EnableForeignKeysAsync();

                        var model = new Mailbox { Id = _mailbox.Id };
                        context.Mailboxes.Remove(model);

                        await context.SaveChangesAsync(
                            OptimisticConcurrencyStrategy.ClientWins);
                    }
                });

                await Task.WhenAll(new[] { deleteFromServer, deleteFromStore });
                _account.RemoveMailboxes(new[] { this });
            } catch (Exception ex) {
                Logger.Error(ex);
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
                            Flags = string.Empty
                        };

                        context.Mailboxes.Add(model);
                        await context.SaveChangesAsync(OptimisticConcurrencyStrategy.ClientWins);
                        return model;
                    }
                });
                
                Account.AddMailboxes(new[] { new MailboxContext(Account, await createInStore) });

                await createOnServer;
                IsExpanded = true;
            } catch (Exception ex) {
                Logger.Error(ex);
            } finally {
                Logger.Exit();
            }
        }

        public bool CanHaveChildren {
            get { return !_mailbox.Flags.ContainsIgnoreCase(MailboxFlags.NoChildren); }
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

        private Task<MailAccount> GetAccountAsync() {
            using (var context = new DatabaseContext()) {
                return context.MailAccounts.FindAsync(_mailbox.AccountId);
            }
        }

        internal async Task<ICollection<MailMessageContext>> SyncMessagesAsync() {
            Logger.Enter();

            Application.Current.AssertUIThread();

            if (IsSyncingMessages) {
                throw new InvalidOperationException("IsSyncingMessages");
            }

            try {
                IsSyncingMessages = true;

                var results = await Task.Run(async () => {
                    var name = _mailbox.Name;
                    var getMaxUid = GetMaxUidAsync();

                    long[] seenUids;
                    long[] unseenUids;
                    List<MailMessage> messages;

                    var getAccount = await GetAccountAsync();
                    using (var connection = new ImapConnection {
                        Security = getAccount.ImapSecurity
                    }) {
                        using (var auth = await connection.ConnectAsync(getAccount.ImapHost, getAccount.ImapPort)) {
                            using (var session = await auth.LoginAsync(getAccount.ImapUsername, getAccount.ImapPassword)) {
                                var mailbox = await session.SelectAsync(name);

                                messages = (await FetchRecentEnvelopesAsync(mailbox, await getMaxUid)).ToList();
                                seenUids = (await mailbox.SearchAsync("1:* SEEN")).ToArray();
                                unseenUids = (await mailbox.SearchAsync("1:* NOT SEEN")).ToArray();
                            }
                        }
                    }

                    if (messages.Count > 0) {
                        await StoreContactsAsync(messages);
                        using (var context = new DatabaseContext()) {
                            await context.ConnectAsync();
                            var transaction = context.Database.Connection.BeginTransaction();
                            try {
                                var mailbox = await context.Mailboxes.FindAsync(_mailbox.Id);
                                mailbox.Messages.AddRange(messages);
                                await context.SaveChangesAsync(OptimisticConcurrencyStrategy.ClientWins);

                                using (var command = context.Database.Connection.CreateCommand()) {
                                    command.CommandText = "INSERT INTO mail_content(text, message_id) VALUES(@text, @message_id);";

                                    foreach (var message in messages) {
                                        var id = message.Id;
                                        var subject = message.Subject;
                                        var fromName = message.FromName;
                                        var fromAddress = message.FromAddress;

                                        command.Parameters.Clear();
                                        command.Parameters.AddRange(new[] { new SQLiteParameter("@text", subject), new SQLiteParameter("@message_id", id) });
                                        command.ExecuteNonQuery();

                                        command.Parameters.Clear();
                                        command.Parameters.AddRange(new[] { new SQLiteParameter("@text", fromAddress), new SQLiteParameter("@message_id", id) });
                                        command.ExecuteNonQuery();

                                        command.Parameters.Clear();
                                        command.Parameters.AddRange(new[] { new SQLiteParameter("@text", fromName), new SQLiteParameter("@message_id", id) });
                                        command.ExecuteNonQuery();
                                    }
                                }

                                transaction.Commit();
                            } catch (Exception ex) {
                                Logger.Error(ex);
                                transaction.Rollback();
                                throw;
                            }
                        }
                    }

                    return new SyncResults {
                        Messages = messages
                            .Select(x => new MailMessageContext(this, x))
                            .ToArray(),
                        UidsForSeen = new HashSet<long>(seenUids),
                        UidsForUnseen = new HashSet<long>(unseenUids)
                    };
                });

                if (results.Messages.Length > 0 && App.Context.SelectedMailbox == this) {
                    App.Context.NotifyMessagesAdded(results.Messages);
                }

                await DetectAndDropDeletedMessagesAsync(results);
                await StoreMessageFlagsAsync(results);
                await CountNotSeenAsync();

                return results.Messages;
            } finally {
                FetchedEnvelopeCount = 0;
                IsSyncingMessages = false;
                Logger.Exit();
            }
        }

        private async Task StoreMessageFlagsAsync(SyncResults results) {
            Logger.Enter();

            try {
                var models = await Task.Run(async () => {
                    using (var context = new DatabaseContext()) {
                        var messages =
                            await
                                context.MailMessages.Where(x => x.MailboxId == Id)
                                    .ToArrayAsync();
                        foreach (var message in messages) {
                            var isSeen = results.UidsForSeen.Contains(message.Uid);
                            if (
                                message.Flags.Any(
                                    x => x.Value == MailMessageFlags.Seen) == isSeen) {
                                continue;
                            }

                            if (isSeen) {
                                message.Flags.Add(new MessageFlag {
                                    Value = MailMessageFlags.Seen
                                });
                            } else {
                                message.Flags.RemoveAll(
                                    x => x.Value == MailMessageFlags.Seen);
                            }
                        }
                        await context.SaveChangesAsync();
                        return messages;
                    }
                });

                var dictionary = models.ToDictionary(model => model.Id);
                App.Context.NotifySeenStatesChanged(dictionary);
            } catch (Exception ex) {
                Logger.Error(ex);
            } finally {
                Logger.Exit();
            }
        }

        private async Task DetectAndDropDeletedMessagesAsync(SyncResults results) {
            Logger.Enter();

            try {
                var uids = new HashSet<long>(results.UidsForSeen.Concat(results.UidsForUnseen).Distinct());
                if (uids.Count == 0) {
                    return;
                }

                var deletedIds = await Task.Run(async () => {
                    using (var context = new DatabaseContext()) {
                        await context.EnableForeignKeysAsync();

                        var messages = await context.MailMessages
                            .Where(x => x.MailboxId == Id)
                            .ToArrayAsync();

                        var deletedMessages = messages
                            .Where(x => !uids.Contains(x.Uid))
                            .ToArray();

                        foreach (var deletedMessage in deletedMessages) {
                            context.MailMessages.Remove(deletedMessage);
                        }

                        await
                            context.SaveChangesAsync(
                                OptimisticConcurrencyStrategy.ClientWins);
                        return deletedMessages.Select(x => x.Id).ToArray();
                    }
                });

                if (deletedIds.Length > 0 && App.Context.SelectedMailbox == this) {
                    App.Context.NotifyMessagesRemoved(deletedIds);
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            } finally {
                Logger.Exit();
            }
        }

        public int TotalEnvelopeCount {
            get { return _totalEnvelopeCount; }
            set {
                if (_totalEnvelopeCount == value) {
                    return;
                }
                _totalEnvelopeCount = value;
                RaisePropertyChanged(() => TotalEnvelopeCount);
            }
        }

        public int FetchedEnvelopeCount {
            get { return _fetchedEnvelopeCount; }
            set {
                if (_fetchedEnvelopeCount == value) {
                    return;
                }
                _fetchedEnvelopeCount = value;
                RaisePropertyChanged(() => FetchedEnvelopeCount);
            }
        }

        private async Task<IEnumerable<MailMessage>> FetchRecentEnvelopesAsync(ImapMailbox mailbox, long uid) {
            var criteria = string.Format("{0}:*", uid);
            var uids = await mailbox.SearchAsync(criteria);

            if (!uids.Any()) {
                return new MailMessage[0];
            }

            FetchedEnvelopeCount = 0;
            TotalEnvelopeCount = uids.Count;

            mailbox.EnvelopeFetched += OnEnvelopeFetched;
            var envelopes = await mailbox.FetchEnvelopesAsync(uids);
            mailbox.EnvelopeFetched -= OnEnvelopeFetched;

            var duplicate = envelopes.FirstOrDefault(x => x.Uid == uid);
            if (duplicate != null) {
                envelopes.Remove(duplicate);
            }

            var messages = new List<MailMessage>();
            foreach (var envelope in envelopes) {
                try {
                    var message = envelope.ToMailMessage();
                    messages.Add(message);
                } catch (Exception ex) {
                    Logger.Error(ex);
                }
            }
            return messages;
        }

        private void OnEnvelopeFetched(object sender, EnvelopeFetchedEventArgs e) {
            FetchedEnvelopeCount++;
        }

        private static async Task StoreContactsAsync(IEnumerable<MailMessage> messages) {
            Logger.Enter();

            Application.Current.AssertBackgroundThread();

            try {
                var groups = messages.GroupBy(x => x.FromAddress).ToArray();
                var contacts = await Task.Run(() => {
                    using (var context = new DatabaseContext()) {
                        return context.MailContacts
                            .GroupBy(x => x.Address)
                            .ToArrayAsync();
                    }
                });

                var diff = groups
                    .Where(x => contacts
                        .All(y => string.Compare(x.Key, y.Key,
                            StringComparison.InvariantCultureIgnoreCase) != 0))
                    .ToArray();

                await Task.Run(async () => {
                    using (var context = new DatabaseContext()) {
                        foreach (var model in diff.Select(group => new MailContact {
                            Address = group.First().FromAddress,
                            Name = group.First().FromName
                        })) {
                            context.MailContacts.Add(model);
                        }

                        if (diff.Length < 1) {
                            return;
                        }

                        await context.SaveChangesAsync();
                    }
                });
            } catch (Exception ex) {
                Logger.Error(ex);
            } finally {
                Logger.Exit();
            }
        }

        internal async Task MarkAsNotSeenAsync(MailMessageContext[] messages) {
            Application.Current.AssertBackgroundThread();

            try {
                messages.ForEach(x => x.IsSeen = false);
                var uids = messages.Select(x => x.Uid).ToArray();

                using (var connection = new ImapConnection { Security = _account.ImapSecurity }) {
                    connection.RemoteCertificateValidationFailed += (sender, e) => e.IsCanceled = false;
                    using (var auth = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                        using (var session = await auth.LoginAsync(_account.ImapUsername, _account.ImapPassword)) {
                            var folder = await session.SelectAsync(Name);
                            await folder.MarkAsNotSeenAsync(uids);
                        }
                    }
                }

                await CountNotSeenAsync();
            } catch (Exception ex) {
                messages.ForEach(x => x.IsSeen = true);
                Logger.Error(ex);
            }
        }

        internal async Task MarkAsSeenAsync(MailMessageContext[] messages) {
            Application.Current.AssertBackgroundThread();

            try {
                messages.ForEach(x => x.IsSeen = true);
                var uids = messages.Select(x => x.Uid).ToArray();

                using (var connection = new ImapConnection { Security = _account.ImapSecurity }) {
                    using (var auth = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                        using (var session = await auth.LoginAsync(_account.ImapUsername, _account.ImapPassword)) {
                            var folder = await session.SelectAsync(Name);
                            await folder.MarkAsSeenAsync(uids);
                        }
                    }
                }

                await CountNotSeenAsync();
            } catch (Exception ex) {
                messages.ForEach(x => x.IsSeen = false);
                Logger.Error(ex);
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

                await CountNotSeenAsync();
            } catch (Exception ex) {
                messages.ForEach(x => x.IsSeen = true);
                Logger.Error(ex);
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

                await CountNotSeenAsync();
            } catch (Exception ex) {
                messages.ForEach(x => x.IsSeen = true);
                Logger.Error(ex);
            }
        }

        internal async Task CountNotSeenAsync() {
            Logger.Enter();

            Application.Current.AssertUIThread();

            try {
                NotSeenCount = await Task.Run(() => {
                    using (var context = new DatabaseContext()) {
                        return context.MailMessages
                            .Where(x => x.MailboxId == _mailbox.Id)
                            .Where(x => x.Flags.All(y => y.Value != MailMessageFlags.Seen))
                            .CountAsync();
                    }
                });
            } catch (Exception ex) {
                Logger.Error(ex);
            } finally {
                Logger.Exit();
            }
        }

        private Task<Int64> GetMaxUidAsync() {
            return Task.Run(() => {
                using (var context = new DatabaseContext()) {
                    return context.MailMessages
                        .Where(x => x.MailboxId == _mailbox.Id)
                        .Select(x => x.Uid)
                        .DefaultIfEmpty(1)
                        .MaxAsync(x => x);
                }
            });
        }

        //internal async Task BindMailboxAsync(ImapMailboxInfo mailbox, IEnumerable<ImapMailboxInfo> subscriptions) {
        //    try {
        //        using (var context = new DatabaseContext()) {
        //            context.Mailboxes.Attach(_mailbox);
        //            context.Entry(_mailbox).State = EntityState.Modified;

        //            _mailbox.Name = mailbox.Fullname;
        //            _mailbox.Delimiter = mailbox.Delimiter.ToString(CultureInfo.InvariantCulture);
        //            _mailbox.Flags = mailbox.Flags.Aggregate((c, n) => c + ';' + n);
        //            _mailbox.IsSubscribed = IsSubscribed || subscriptions.Any(x => x.Name == mailbox.Name);

        //            await context.SaveChangesAsync();
        //        }
        //    } catch (Exception ex) {
        //        Logger.Error(ex);
        //    }
        //}

        internal async Task<MailMessageContext[]> QueryAsync(string text) {
            using (var context = new DatabaseContext()) {
                var messages = await context.MailMessages
                    .Where(x => x.Subject.Contains(text) && x.MailboxId == Id)
                    .ToArrayAsync();

                return messages.Select(x => new MailMessageContext(this, x)).ToArray();
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

        internal async void IdleAsync() {
            Application.Current.AssertUIThread();

            IsIdling = true;

            await Task.Run(async () => {
                Thread.CurrentThread.Name = string.Format("IDLE Thread ({0})", Name);
                try {
                    using (var connection = new ImapConnection { Security = _account.ImapSecurity }) {
                        using (var auth = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                            using (var session = await auth.LoginAsync(_account.ImapUsername, _account.ImapPassword)) {
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
                } catch (Exception ex) {
                    Logger.Error(ex);
                }
            });

            IsIdling = false;
        }

        private async void OnChangeNotificationReceived(object sender, EventArgs e) {
            Application.Current.AssertBackgroundThread();

            Logger.Info(Resources.IdleChangeNotification);

            await Application.Current.Dispatcher.InvokeAsync(async () => {
                try {
                    if (IsSyncingMessages) {
                        return;
                    }

                    var messages = await SyncMessagesAsync();
                    if (messages.Count <= 0)
                        return;

                    var notification =
                        new NotificationWindow(messages) {
                            ShowActivated = false
                        };

                    notification.Show();
                } catch (Exception ex) {
                    Logger.Error(ex);
                }
            });
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
                Logger.Error(ex);
            }
        }



        internal async Task MarkAsNotFlaggedAsync(MailMessageContext[] messages) {
            Application.Current.AssertBackgroundThread();

            try {
                messages.ForEach(x => x.IsFlagged = false);
                var uids = messages.Select(x => x.Uid).ToArray();

                using (var connection = new ImapConnection { Security = _account.ImapSecurity }) {
                    using (var auth = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                        using (var session = await auth.LoginAsync(_account.ImapUsername, _account.ImapPassword)) {
                            var folder = await session.SelectAsync(Name);
                            await folder.MarkAsNotFlaggedAsync(uids);
                        }
                    }
                }

                await CountNotSeenAsync();
            } catch (Exception ex) {
                messages.ForEach(x => x.IsFlagged = true);
                Logger.Error(ex);
            }
        }

        #region Nested Structures

        private struct SyncResults {
            public MailMessageContext[] Messages { get; set; }
            public HashSet<long> UidsForSeen { get; set; }
            public HashSet<long> UidsForUnseen { get; set; }
        }

        #endregion

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

            MailMessage[] messages;

            var t1 = Environment.TickCount & Int32.MaxValue;

            using (var context = new DatabaseContext()) {
                if (!App.Context.ShowOnlyUnseen) {
                    messages = await context.MailMessages
                        .Include(x => x.Flags)
                        .Where(x => x.MailboxId == _mailbox.Id)
                        .ToArrayAsync();
                } else {
                    messages = await context.MailMessages
                        .Include(x => x.Flags)
                        .Where(x => x.Flags.All(y => y.Value != MailMessageFlags.Seen))
                        .Where(x => x.MailboxId == _mailbox.Id)
                        .ToArrayAsync();
                }
            }

            var t2 = Environment.TickCount & Int32.MaxValue;
            Logger.Debug("MailboxContext::GetMessagesAsync duration: {0} seconds.", (t2 - t1) / 1000);

            return messages.Select(x => new MailMessageContext(this, x));
        }

        #endregion
    }
}
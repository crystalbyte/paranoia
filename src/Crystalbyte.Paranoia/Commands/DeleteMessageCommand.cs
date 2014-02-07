#region Using directives

using System;
using System.Composition;
using System.Linq;
using System.Windows.Input;
using Crystalbyte.Paranoia.Contexts;
using Crystalbyte.Paranoia.Messaging;

#endregion

namespace Crystalbyte.Paranoia.Commands {
    [Export, Shared]
    public sealed class DeleteMessageCommand : ICommand {
        [Import]
        public AppContext AppContext { get; set; }

        [Import]
        public ImapMessageSelectionSource ImapMessageSelectionSource { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            ImapMessageSelectionSource.CollectionChanged += (sender, e) => OnCanExecuteChanged(EventArgs.Empty);
        }

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return ImapMessageSelectionSource.SelectedMessages.Any();
        }

        public async void Execute(object parameter) {
            var messages = ImapMessageSelectionSource.SelectedMessages.GroupBy(x => x.Account.Host);
            foreach (var host in messages) {
                var users = host.GroupBy(x => x.Account.ImapUsername);
                foreach (var user in users) {
                    var sample = user.First();
                    using (var connection = new ImapConnection {Security = SecurityPolicy.Implicit}) {
                        var authenticator =
                            await connection.ConnectAsync(sample.Account.Host, sample.Account.ImapPort);
                        var session =
                            await authenticator.LoginAsync(sample.Account.ImapUsername, sample.Account.Password);

                        var mailboxes = user.GroupBy(x => x.Mailbox);
                        foreach (var mailbox in mailboxes) {
                            var box = await session.SelectAsync(mailbox.Key);
                            try {
                                await box.DeleteAsync(mailbox.Select(x => x.Uid));
                            }
                            catch (Exception) {
                                // TODO: Log
                            }
                        }
                    }
                }
            }

            AppContext.SyncAsync();
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged(EventArgs e) {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, e);
        }

        #endregion
    }
}
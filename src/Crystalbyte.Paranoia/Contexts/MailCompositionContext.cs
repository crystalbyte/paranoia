using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.UI.Commands;

namespace Crystalbyte.Paranoia {
    public sealed class MailCompositionContext : NotificationObject {

        private string _text;
        private string _subject;
        private readonly ObservableCollection<string> _recipients;
        private readonly ObservableCollection<MailContactContext> _suggestions;
        private readonly ICommand _sendCommand;

        public MailCompositionContext() {
            _recipients = new ObservableCollection<string>();
            _suggestions = new ObservableCollection<MailContactContext>();
            _sendCommand = new SendCommand(this);
        }

        public ICommand SendCommand {
            get { return _sendCommand; }
        }

        public ICollection<string> Recipients {
            get { return _recipients; }
        }

        public IEnumerable<MailContactContext> Suggestions {
            get { return _suggestions; }
        }

        public string Subject {
            get { return _subject; }
            set {
                if (_subject == value) {
                    return;
                }

                _subject = value;
                RaisePropertyChanged(() => Subject);
            }
        }

        public string Text {
            get { return _text; }
            set {
                if (_text == value) {
                    return;
                }
                _text = value;
                RaisePropertyChanged(() => Text);
            }
        }

        public async Task QueryRecipientsAsync(string text) {
            var account = App.Context.SelectedAccount;

            using (var database = new DatabaseContext()) {
                var candidates = await database.MailContacts
                    .Where(x => x.AccountId == account.Id)
                    .Where(x => x.Address.StartsWith(text) || x.Name.StartsWith(text))
                    .ToArrayAsync();

                _suggestions.Clear();
                _suggestions.AddRange(candidates.Select(x => new MailContactContext(x)));
            }
        }

        public async Task ResetAsync() {
            _recipients.Clear();
            Subject = string.Empty;

            var info = Application.GetResourceStream(new Uri("Resources/composition.template.html", UriKind.Relative));
            if (info != null) {
                using (var reader = new StreamReader(info.Stream)) {
                    Text = await reader.ReadToEndAsync();
                }
            }
        }

        public void SendAsync() {

        }
    }
}

﻿using System;
using System.Linq;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class SendCommand  : ICommand{
        private readonly MailCompositionContext _composition;

        public SendCommand(MailCompositionContext composition) {
            _composition = composition;
        }

        public bool CanExecute(object parameter) {
            return !string.IsNullOrEmpty(_composition.Subject)
                   || !_composition.Recipients.Any();
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null) 
                handler(this, EventArgs.Empty);
        }

        public async void Execute(object parameter) {
            _composition.Finish();
            await _composition.PushToOutboxAsync();
        }
    }
}

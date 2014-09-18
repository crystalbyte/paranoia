using Microsoft.Win32;
using System;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI.Commands {
    internal class AddAttachmentCommand : ICommand {

        private readonly MailCompositionContext _context;

        public AddAttachmentCommand(MailCompositionContext context) {
            _context = context;
        }

        public bool CanExecute(object parameter) {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public virtual void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void Execute(object parameter) {
            var dialoge = new OpenFileDialog { Multiselect = true };
            var result = dialoge.ShowDialog();

            if (result == null || result != true)
                return;

            var filename = dialoge.FileNames;
            filename.ForEach(x => _context.Attachments.Add(new AttachmentContext(_context, x)));
        }
    }
}

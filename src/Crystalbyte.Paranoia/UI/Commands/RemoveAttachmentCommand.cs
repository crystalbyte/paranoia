using System;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI.Commands {
    public class RemoveAttachmentCommand : ICommand {

        private readonly MailCompositionContext _context;
        private readonly AttachmentContext _attachmentContext;

        public RemoveAttachmentCommand(MailCompositionContext context, AttachmentContext attachmentContext) {
            _context = context;
            _attachmentContext = attachmentContext;
        }

        public bool CanExecute(object parameter) {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter) {
            _context.Attachments.Remove(_attachmentContext);
        }
    }
}

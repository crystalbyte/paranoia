using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public void Execute(object parameter) {
            var dialoge = new OpenFileDialog();
            dialoge.Multiselect = true;
            var result = dialoge.ShowDialog();

            if (result != null && result == true) {
                var filename = dialoge.FileNames;
                filename.ForEach(x => _context.Attachments.Add(new AttachmentContext(_context, x)));
            }
        }
    }
}

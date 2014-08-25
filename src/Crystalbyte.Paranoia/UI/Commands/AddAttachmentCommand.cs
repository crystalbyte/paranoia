using Crystalbyte.Paranoia.Contexts;
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

            Nullable<bool> result = dialoge.ShowDialog();

            if (result != null && result == true) {
                string filename = dialoge.FileName;
                _context.Attachments.Add(new AttachmentContext(filename));
            }
        }
    }
}

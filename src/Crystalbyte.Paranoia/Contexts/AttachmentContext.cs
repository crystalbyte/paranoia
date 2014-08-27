using System;
using System.Linq;
using System.Windows.Input;
using Crystalbyte.Paranoia.Mail.Mime;
using Crystalbyte.Paranoia.UI.Commands;

namespace Crystalbyte.Paranoia {
    public class AttachmentContext {

        private readonly string _name;
        private readonly string _fullname;
        private readonly MessagePart _part;
        private readonly RemoveAttachmentCommand _removeCommand;
        private readonly OpenAttachmentCommand _openCommand;
        private readonly MailCompositionContext _context;

        public AttachmentContext(MailCompositionContext context, string fullname) {
            _context = context;
            _fullname = fullname;
            _name = fullname.Split('\\').Last();
            _removeCommand = new RemoveAttachmentCommand(context, this);
        }

        public AttachmentContext(MessagePart part) {
            _part = part;
            if (!part.IsAttachment)
                throw new InvalidOperationException("part must be an attachment");

            _name = part.FileName;
            _fullname = part.FileName;
            _openCommand = new OpenAttachmentCommand(part);
        }

        public void Open() {
            if (_openCommand != null && _openCommand.CanExecute(null)) {
                _openCommand.Execute(null);        
            }
        }

        public string Name {
            get { return _name; }
        }

        public string FullName {
            get { return _fullname; }
        }

        public ICommand RemoveCommand {
            get { return _removeCommand; }
        }

        public ICommand OpenCommand {
            get { return _openCommand; }
        }
    }
}

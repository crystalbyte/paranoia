using Crystalbyte.Paranoia.Mail.Mime;
using Crystalbyte.Paranoia.UI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.Contexts {
    public class AttachmentContext {

        private string _name;
        private string _fullName;
        private RemoveAttachmentCommand _removeAttachmentCommand;
        private OpenAttachmentCommand _openAttachmentCommand;
        private readonly MailCompositionContext _context;
        private Mail.Mime.MessagePart x;

        public AttachmentContext(MailCompositionContext context, string fileName) {
            _context = context;
            _fullName = fileName;
            _name = fileName.Split('\\').Last();
            _removeAttachmentCommand = new RemoveAttachmentCommand(context, this);
        }

        public AttachmentContext(MessagePart part) {
            if (!part.IsAttachment)
                throw new InvalidOperationException("part must be an attachment");

            _name = part.FileName;
            _fullName = part.FileName;
            _openAttachmentCommand = new OpenAttachmentCommand(part);
        }

        public string Name {
            get { return _name; }
        }

        public string FullName {
            get { return _fullName; }
        }

        public ICommand RemoveAttachmentCommand {
            get { return _removeAttachmentCommand; }
        }

        public ICommand OpenAttachmentCommand {
            get { return _openAttachmentCommand; }
        }
    }
}

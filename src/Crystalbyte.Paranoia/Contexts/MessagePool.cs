using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Crystalbyte.Paranoia.Data;

namespace Crystalbyte.Paranoia {
    internal sealed class MessagePool {

        private readonly Stack<MailMessageContext> _vacantObjects = new Stack<MailMessageContext>();

        public MessagePool() {
            GenerateObjects(256);
        }

        private void GenerateObjects(int count) {
            for (var i = 0; i < count; i++) {
                _vacantObjects.Push(new MailMessageContext());
            }
        }

        internal void Recycle(MailMessageContext message) {
            Application.Current.AssertUIThread();

            message.Recycle();
            _vacantObjects.Push(message);
        }

        internal MailMessageContext Request(MailboxContext mailbox, MailMessageModel message) {
            Application.Current.AssertUIThread();

            var context = Request(1).First();
            context.Bind(mailbox, message);
            return context;
        }

        internal IEnumerable<MailMessageContext> Request(int count) {
            Application.Current.AssertUIThread();

            if (_vacantObjects.Count <= count) {
                for (var i = 0; i < _vacantObjects.Count; i++) {
                    _vacantObjects.Push(new MailMessageContext());  
                }
            }

            for (var i = 0; i < count; i++) {
                yield return _vacantObjects.Pop();                                                              
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Crystalbyte.Paranoia.Data;

namespace Crystalbyte.Paranoia {
    internal sealed class MessagePool {

        private readonly Stack<MailMessageContext> _vacantObjects = new Stack<MailMessageContext>();
        private readonly HashSet<MailMessageContext> _occupiedObjects = new HashSet<MailMessageContext>();

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

            //message.Recycle();

            var success = _occupiedObjects.Remove(message);
            if (!success) {
                throw new InvalidOperationException();
            }

            _vacantObjects.Push(message);
        }

        internal MailMessageContext Request(MailboxContext mailbox, MailMessageModel message) {
            Application.Current.AssertUIThread();

            var context = Request(1).First();
            //context.Bind(mailbox, message);
            return context;
        }

        internal IEnumerable<MailMessageContext> Request(int count) {
            Application.Current.AssertUIThread();

            if (_vacantObjects.Count <= count) {
                var total = _vacantObjects.Count + _occupiedObjects.Count;
                for (var i = 0; i < total; i++) {
                    _vacantObjects.Push(new MailMessageContext());  
                }
            }

            for (var i = 0; i < count; i++) {
                var obj = _vacantObjects.Pop();                                                              
                _occupiedObjects.Add(obj);
                yield return obj;
            }
        }
    }
}

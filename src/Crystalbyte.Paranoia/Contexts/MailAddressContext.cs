using System;
using System.Windows;
using System.Windows.Input;
using Crystalbyte.Paranoia.Data;
using System.Diagnostics;
using Crystalbyte.Paranoia.UI.Commands;
using NLog;

namespace Crystalbyte.Paranoia {
    [DebuggerDisplay("Filename = {Filename}, MailAddress = {MailAddress}")]
    public sealed class MailAddressContext {

        #region Private Fields

        private readonly MailAddress _address;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        internal MailAddressContext(MailAddress address) {
            _address = address;
            CopyAddress = new RelayCommand(OnCopyAddress);
        }

        #endregion

        #region Properties

        public string Name {
            get { return _address.Name; }
        }

        public string Address {
            get { return _address.Address; }
        }

        public AddressRole Role {
            get { return _address.Role; }
        }

        public ICommand CopyAddress { get; set; }

        #endregion

        #region Methods

        private void OnCopyAddress(object obj) {
            try {
                Clipboard.SetText(Address);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        #endregion
    }
}

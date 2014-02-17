using Crystalbyte.Paranoia.Contexts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Crystalbyte.Paranoia.Converters {
    public sealed class MailboxToImageConverter : IValueConverter {

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var mailbox = value as MailboxContext;
            if (mailbox == null) {
                return null;
            }

            if (mailbox.IsInbox) {
                return "/Assets/download.png";
            }

            if (mailbox.IsJunk) {
                return "/Assets/questionmark.png";
            }

            if (mailbox.IsSent) {
                return "/Assets/upload.png";
            }

            if (mailbox.IsDraft) {
                return "/Assets/edit.png";
            }

            if (mailbox.IsTrash) {
                return "/Assets/delete.png";
            }

            if (mailbox.IsImportant) {
                return "/Assets/important.png";
            }

            if (mailbox.IsFlagged) {
                return "/Assets/favs.png";
            }

            return "/Assets/folder.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return Binding.DoNothing;
        }

        #endregion
    }
}

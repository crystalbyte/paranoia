#region Using directives

using System;
using System.Globalization;
using System.Windows.Data;
using Crystalbyte.Paranoia.Properties;

#endregion

namespace Crystalbyte.Paranoia.UI.Converters {
    [ValueConversion(typeof(MailboxContext), typeof(string))]
    public sealed class MailboxLocalizer : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var context = (MailboxContext)value;
            if (context.IsTrash) {
                return Resources.TrashMailbox;
            }
            if (context.IsDraft) {
                return Resources.DraftMailbox;
            }
            if (context.IsSent) {
                return Resources.SentMailbox;
            }
            if (context.IsJunk) {
                return Resources.JunkMailbox;
            }
            if (context.IsInbox) {
                return Resources.InboxMailbox;
            }

            return context.LocalName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}
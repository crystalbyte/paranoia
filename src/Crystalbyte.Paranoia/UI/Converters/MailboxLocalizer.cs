#region Using directives

using System;
using System.Globalization;
using System.Windows.Data;
using Crystalbyte.Paranoia.Properties;

#endregion

namespace Crystalbyte.Paranoia.UI.Converters {
    [ValueConversion(typeof (MailboxType), typeof (string))]
    public sealed class MailboxLocalizer : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var type = (MailboxType) value;
            switch (type) {
                case MailboxType.Inbox:
                    return Resources.InboxMailbox;
                case MailboxType.Sent:
                    return Resources.SentMailbox;
                case MailboxType.Draft:
                    return Resources.DraftMailbox;
                case MailboxType.Trash:
                    return Resources.TrashMailbox;
                case MailboxType.All:
                    return Resources.AllMailbox;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}
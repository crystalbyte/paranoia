using Crystalbyte.Paranoia.Properties;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Crystalbyte.Paranoia.UI.Converters {
    [ValueConversion(typeof(MailboxType), typeof(string))]
    public sealed class MailboxLocalizer : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var type = (MailboxType)value;
            switch (type) {
                case MailboxType.Inbox:
                    return Resources.InboxMailbox;
                case MailboxType.Sent:
                    return Resources.SentMailbox;
                case MailboxType.Draft:
                    return Resources.DraftMailbox;
                case MailboxType.Trash:
                    return Resources.TrashMailbox;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}

#region Using directives

using System;
using System.Globalization;
using System.Windows.Data;
using Crystalbyte.Paranoia.Properties;

#endregion

namespace Crystalbyte.Paranoia.UI.Converters {
    [ValueConversion(typeof (MailboxContext), typeof (string))]
    public sealed class MailboxLocalizer : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var context = (MailboxContext) value;
            switch (context.Type) {
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
                case MailboxType.Undefined:
                    return string.Format(context.LocalName);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}
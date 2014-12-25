using Crystalbyte.Paranoia.Properties;

namespace Crystalbyte.Paranoia {
    public enum SortProperty {
        [Description(Type = typeof(Resources), Name = "Date")]
        Date = 0,
        [Description(Type = typeof(Resources), Name = "Subject")]
        Subject,
        [Description(Type = typeof(Resources), Name = "Size")]
        Size,
        [Description(Type = typeof(Resources), Name = "Attachments")]
        Attachments
    }
}

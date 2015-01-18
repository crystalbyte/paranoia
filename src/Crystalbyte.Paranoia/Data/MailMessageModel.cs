#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace Crystalbyte.Paranoia.Data {
    [Table("mail_message")]
    public class MailMessageModel {
        private ICollection<MimeMessageModel> _mimeMessages;

        public MailMessageModel() {
            _mimeMessages = new Collection<MimeMessageModel>();
        }

        [Key]
        [Column("id")]
        public Int64 Id { get; set; }

        [Column("uid")]
        public Int64 Uid { get; set; }

        [Column("size")]
        public Int64 Size { get; set; }

        [Column("flags")]
        [Collate(CollatingSequence.NoCase)]
        public string Flags { get; set; }

        [Column("subject")]
        [Collate(CollatingSequence.NoCase)]
        public string Subject { get; set; }

        [Column("has_attachments")]
        public bool HasAttachments { get; set; }

        [Column("entry_date")]
        public DateTime EntryDate { get; set; }

        [Column("thread_id")]
        public string ThreadId { get; set; }

        [Column("message_id")]
        public string MessageId { get; set; }

        [Column("from_name")]
        [Collate(CollatingSequence.NoCase)]
        public string FromName { get; set; }

        [Column("from_address")]
        [Collate(CollatingSequence.NoCase)]
        public string FromAddress { get; set; }

        [Column("mailbox_id")]
        [ForeignKey("Mailbox")]
        public Int64 MailboxId { get; set; }

        public MailboxModel Mailbox { get; set; }

        public virtual ICollection<MimeMessageModel> MimeMessages {
            get { return _mimeMessages; }
            set { _mimeMessages = value; }
        }
    }
}
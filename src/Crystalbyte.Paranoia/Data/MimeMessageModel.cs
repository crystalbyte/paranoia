using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Data {

    [Table("mime_message")]
    public class MimeMessageModel {

        [Key]
        [Column("id")]
        public Int64 Id { get; set; }

        [Column("message_id")]
        [ForeignKey("Message")]
        public Int64 MessageId { get; set; }

        [Column("data")]
        public string Data { get; set; }

        public MailMessageModel Message { get; set; }
    }
}

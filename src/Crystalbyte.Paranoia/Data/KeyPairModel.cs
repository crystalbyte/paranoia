using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crystalbyte.Paranoia.Data {
    [Table("key_pair")]
    public sealed class KeyPairModel {
        [Key]
        [Column("id")]
        public Int64 Id { get; set; }

        [Column("public_key")]
        public byte[] PublicKey { get; set; }

        [Column("private_key")]
        public byte[] PrivateKey { get; set; }
        
        [Column("date")]
        [Default(DatabaseFunction.CurrentTimestamp)]
        public DateTime Date { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Data {
    [Table("Users")]
    public class User {

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string MailAddress { get; set; }

        public ICollection<Contact> Contacts { get; set; }
    }
}

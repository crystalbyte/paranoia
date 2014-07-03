using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Data {
    internal sealed class Storage : IStorage {

        private readonly StorageContext _context;

        public Storage(StorageContext context) {
            _context = context;
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Cryptography {
    public interface ICryptoAlgorithm {

        string Name { get; }

        void Decrypt();

        void Encrypt();
    }
}

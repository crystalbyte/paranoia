﻿using System;
using System.Data.Entity;
using System.Linq;
using Crystalbyte.Paranoia.Cryptography;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using System.Collections.Generic;

namespace Crystalbyte.Paranoia {
    public sealed class SodiumHybridMimeCypher {

        internal byte[] Decrypt(byte[] bytes, SodiumMimeDecryptionMetadata data) {
            throw new NotImplementedException();            
        }

        internal SodiumMimeEncryptionMetadata Encrypt(MailContact contact, byte[] bytes) {
            var scc = new SecretKeyCrypto();
            var message = scc.Encrypt(bytes);

            var data = new SodiumMimeEncryptionMetadata {
                Version = "1.0",
                EncryptedMessage = message,
                Nonce = scc.Nonce,
                Secret = scc.Key
            };

            data.Entries.AddRange(contact.Keys.AsParallel().Select(x => {
                var pkc = new PublicKeyCrypto();
                var nonce = PublicKeyCrypto.GenerateNonce();
                return new EncryptedSecret {
                    Bytes = pkc.EncryptWithPublicKey(scc.Key, x.Bytes, nonce),
                    Contact = contact,
                    Nonce = nonce
                };
            }));

            return data;
        }
    }
}


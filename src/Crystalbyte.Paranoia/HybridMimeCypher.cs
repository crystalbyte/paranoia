﻿using System;
using System.Data.Entity;
using System.Linq;
using Crystalbyte.Paranoia.Cryptography;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using System.Collections.Generic;

namespace Crystalbyte.Paranoia {
    public sealed class HybridMimeCypher {

        public byte[] Decrypt(byte[] bytes) {


            return bytes;

            //var reader = new MailMessageReader(bytes);
            //var parts = reader.FindAllMessagePartsWithMediaType(MediaTypes.EncryptedMime);
            //if (parts == null || parts.Count == 0) {
            //    return bytes;
            //}

            //var address = reader.Headers.From.Address;
            //var xHeaders = reader.Headers.UnknownHeaders;

            //byte[] pKey = null;
            //byte[] nonce = null;
            //var n = (xHeaders.GetValues(MessageHeaders.Nonce) ?? new[] { string.Empty }).FirstOrDefault();
            //if (!string.IsNullOrEmpty(n)) {
            //    nonce = Convert.FromBase64String(n);
            //}

            //for (var i = 0; i < xHeaders.Count; i++) {
            //    var key = xHeaders.GetKey(i);
            //    if (!key.EqualsIgnoreCase(MessageHeaders.Signet))
            //        continue;

            //    var values = xHeaders.GetValues(i);
            //    if (values == null) {
            //        throw new SignetMissingOrCorruptException(address);
            //    }

            //    var signet = values.First();
            //    var split = signet.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            //    var p = split[0].Substring(split[0].IndexOf('=') + 1).Trim(';');
            //    pKey = Convert.FromBase64String(p);
            //}

            //var part = parts.First();
            //using (var context = new DatabaseContext()) {
            //    var current = context.KeyPairs.OrderByDescending(x => x.Date).First();
            //    var contact = context.MailContacts
            //        .Include(x => x.Keys)
            //        .FirstOrDefault(x => x.Address == address);

            //    if (contact == null) {
            //        throw new MissingContactException(address);
            //    }

            //    if (contact.Keys.Count == 0) {
            //        throw new MissingKeyException(address);
            //    }

            //    var crypto = new PublicKeyCrypto(current.PublicKey, current.PrivateKey);
            //    return crypto.DecryptWithPrivateKey(part.Body, pKey, nonce);
            //}
        }

        internal MimeEncryptionResult Encrypt(MailContact contact, byte[] bytes) {
            var scc = new SecretKeyCrypto();
            var message = scc.Encrypt(bytes);

            var result = new MimeEncryptionResult {
                EncryptedMessage = message,
                Nonce = scc.Nonce,
                Secret = scc.Key
            };

            result.Entries.AddRange(contact.Keys.AsParallel().Select(x => {
                var pkc = new PublicKeyCrypto();
                var nonce = PublicKeyCrypto.GenerateNonce();
                return new PublicKeyEncryptionEntry {
                    EncryptedSecret = pkc.EncryptWithPublicKey(scc.Key, x.Bytes, nonce),
                    Contact = contact,
                    Nonce = nonce
                };
            }));

            return result;
        }
    }
}

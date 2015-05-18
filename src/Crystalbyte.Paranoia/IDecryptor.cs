namespace Crystalbyte.Paranoia {
    internal interface IDecryptor {
        byte[] Decrypt(byte[] bytes);
    }
}

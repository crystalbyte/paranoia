#region Using directives

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

#endregion

namespace Crystalbyte.Paranoia.Cryptography
{
    /// <summary>
    ///   Implementation of a .NET wrapper for the OpenSSL RSA encryption functions. 
    ///   http://www.openssl.org/docs/crypto/rsa.html
    /// </summary>
    public sealed class RsaEncryption : NativeResource
    {
        private readonly IntPtr _handle;

        public RsaEncryption()
        {
            _handle = NativeMethods.RSA_new();
        }

        public void GenerateRSA() {

            var cipher = new EVP_CIPHER();
            var bignum = new BigNum();
            var rsaKeyPair = NativeMethods.RSA_new();
            var keySize = 2048;

            var chiperHandle = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(EVP_CIPHER)));
            Marshal.StructureToPtr(cipher, chiperHandle, false);

            var bignumHandle = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BigNum)));
            Marshal.StructureToPtr(bignum, bignumHandle, false);

            NativeMethods.BN_set_word(bignumHandle, 65537);

            //check entropy
            //free stuff

	        NativeMethods.RSA_generate_key_ex(rsaKeyPair, keySize, bignumHandle, IntPtr.Zero);
        }

        //int pass_cb(char *buf, int size, int rwflag, void *u);
        //   {
        //   int len;
        //   char *tmp;
        //   /* We'd probably do something else if 'rwflag' is 1 */
        //   printf("Enter pass phrase for \"%s\"\n", u);
        //   /* get pass phrase, length 'len' into 'tmp' */
        //   tmp = "hello";
        //   len = strlen(tmp);
        //   if (len <= 0) return 0;
        //   /* if too long, truncate */
        //   if (len > size) len = size;
        //   memcpy(buf, tmp, len);
        //   ret
        //   }


        // Allocates unmanaged memory with the size of a bio struct.
        //var handle = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BIO)));

        // Manged => Unmanaged
        // creates managed struct of type BIO.
        //var bio = new BIO {flags = 0};

        // Copies struct BIO to unmanaged memory at location handle.
        //Marshal.StructureToPtr(bio, handle, false);

        // Unmanaged => Managed
        // Copies struct BIO from unmanaged memory to local variable.
        //bio = (BIO)Marshal.PtrToStructure(handle, typeof (BIO));

        // Free unmanaged memory.
        //Marshal.FreeHGlobal(handle);


        //password handling

        // int pass_cb(char *buf, int size, int rwflag, void *u);
        // {
        //   int len;
        //   char *tmp;
        //   printf("Enter pass phrase for \"%s\"\n", u);
        //   /* get pass phrase, length 'len' into 'tmp' */
        //   tmp = "hello";
        //   len = strlen(tmp);
        //   if (len <= 0) return 0;
        //   /* if too long, truncate */
        //   if (len > size) len = size;
        //   memcpy(buf, tmp, len);
        //   return len;
        //}

        public void RsaPrivateKeyToPem(String key){
            //init buffer or as param
            //var bio = NativeMethods.BIO_new_mem_buf(Buffer, buffer_len);
            //var evp = NativeMethods.EVP_aes_256_cbc();
            
            //assume rsa as object variable
            //key is the user key

            //NativeMethods.PEM_write_bio_RSAPrivateKey(bio, rsa, evp, key, keylen, IntPtr.Zero, IntPtr.Zero);
        }

        public void RsaPublicKeyToPem()
        {
            //init buffer or as param
            //var bio = NativeMethods.BIO_new_mem_buf(Buffer, buffer_len);

            //assume rsa as object variable
            //key is the user key

            //NativeMethods.PEM_write_bio_RSAPublicKey(biop, rsa);
        }

        public void PemToRsaPrivateKey()
        {
            //var rsa = NativeMethods.RSA_new();
            //var bio = NativeMethods.BIO_new_mem_buf(Buffer, buffer_len);
            //NativeMethods.PEM_read_bio_RSAPrivateKey(biop, rsa, password_callback, IntPtr.Zero);
        }

        public void PemToRsaPublicKey()
        {
            //var rsa = NativeMethods.RSA_new();
            //var bio = NativeMethods.BIO_new_mem_buf(Buffer, bufferlen);
            //NativeMethods.PEM_read_bio_RSAPublicKey(bio, rsa, pass_cb, IntPtr.Zero);
        }

        public String Encrypt()
        {
            return "";
            //RSA_public_encrypt(int flen, IntPtr from, IntPtr to, IntPtr rsa, int padding);
        }

        public String Decrypt(String chiper)
        {
            return "";
            //RSA_public_encrypt(int flen, IntPtr from, IntPtr to, IntPtr rsa, int padding);
        }

        protected override void DisposeNative()
        {
            base.DisposeNative();

            if (_handle != IntPtr.Zero)
            {
                NativeMethods.RSA_free(_handle);
            }
        }

        private static class NativeMethods
        {

            // BIO

            [DllImport(OpenSsl.Library, EntryPoint = "BIO_new")]
            public static extern IntPtr BIO_new(IntPtr type);

            [DllImport(OpenSsl.Library, EntryPoint = "BIO_s_mem")]
            public static extern IntPtr BIO_s_mem();

            [DllImport(OpenSsl.Library, EntryPoint = "BIO_Free")]
            public static extern void BIO_Free(IntPtr bio);

            [DllImport(OpenSsl.Library, EntryPoint = "BIO_set_fp")]
            public static extern void BIO_set_fp(IntPtr bio,IntPtr fp, int flags);

            [DllImport(OpenSsl.Library, EntryPoint = "BIO_new_mem_buf")]
            public static extern void BIO_new_mem_buf(IntPtr buf, int len);


            
            // EVP

            [DllImport(OpenSsl.Library, EntryPoint = "EVP_aes_256_cbc")]
            public static extern IntPtr EVP_aes_256_cbc();

            [DllImport(OpenSsl.Library, EntryPoint = "EVP_PKEY_assign_RSA")]
            public static extern int EVP_PKEY_assign_RSA(IntPtr pkey, IntPtr keypair);

            [DllImport(OpenSsl.Library, EntryPoint = "EVP_PKEY_new")]
            public static extern IntPtr EVP_PKEY_new();

            // PEM

            [DllImport(OpenSsl.Library, EntryPoint = "PEM_ASN1_write_bio")]
            public static extern int PEM_ASN1_write_bio(IntPtr i2d, IntPtr name, IntPtr biop, IntPtr x, IntPtr enc, IntPtr kstr, int klen, IntPtr pass_cb, IntPtr u);

            [DllImport(OpenSsl.Library, EntryPoint = "PEM_ASN1_read_bio")]
            public static extern void PEM_ASN1_read_bio(IntPtr d2i, IntPtr name, IntPtr biop, IntPtr x, IntPtr pass_cb, IntPtr u);

            [DllImport(OpenSsl.Library, EntryPoint = "PEM_read_bio_RSAPrivateKey")]
            public static extern IntPtr PEM_read_bio_RSAPrivateKey(IntPtr biop, IntPtr rsa, IntPtr pass_cb, IntPtr u);

            [DllImport(OpenSsl.Library, EntryPoint = "PEM_write_bio_RSAPrivateKey")]
            public static extern int PEM_write_bio_RSAPrivateKey(IntPtr biop, IntPtr rsa, IntPtr enc, IntPtr key, int keylen, IntPtr pass_cb, IntPtr u);

            [DllImport(OpenSsl.Library, EntryPoint = "PEM_read_bio_RSAPublicKey")]
            public static extern IntPtr PEM_read_bio_RSAPublicKey(IntPtr biop, IntPtr rsa, IntPtr pass_cb, IntPtr u);

            [DllImport(OpenSsl.Library, EntryPoint = "PEM_write_bio_RSAPublicKey")]
            public static extern IntPtr PEM_write_bio_RSAPublicKey(IntPtr biop, IntPtr rsa);


            // BN

            [DllImport(OpenSsl.Library, EntryPoint = "BN_new")]
            public static extern IntPtr BN_new();

            [DllImport(OpenSsl.Library, EntryPoint = "BN_free")]
            public static extern void BN_free(IntPtr bignum);

            [DllImport(OpenSsl.Library, EntryPoint = "BN_set_word")]
            public static extern void BN_set_word(IntPtr bignum, ulong w);

            // RSA

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_new")]
            public static extern IntPtr RSA_new();

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_free")]
            public static extern void RSA_free(IntPtr handle);

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_generate_key_ex")]
            public static extern IntPtr RSA_generate_key_ex(IntPtr rsa, int bits, IntPtr e, IntPtr cb);

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_public_encrypt")]
            public static extern int RSA_public_encrypt(int flen, IntPtr from, IntPtr to, IntPtr rsa, int padding);

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_public_decrypt")]
            public static extern int RSA_public_decrypt(int flen, IntPtr from, IntPtr to, IntPtr rsa, int padding);

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_private_encrypt")]
            public static extern int RSA_private_encrypt(int flen, IntPtr from, IntPtr to, IntPtr rsa, int padding);

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_private_decrypt")]
            public static extern int RSA_private_decrypt(int flen, IntPtr from, IntPtr to, IntPtr rsa, int padding);

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_sign")]
            public static extern int RSA_sign(int type, IntPtr m, uint m_len, IntPtr sigret, IntPtr siglen, IntPtr rsa);

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_verify")]
            public static extern int RSA_verify(int type, IntPtr m, uint m_len, IntPtr sigbuf, uint siglen, IntPtr rsa);

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_size")]
            public static extern int RSA_size(IntPtr rsa);

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_check_key")]
            public static extern int RSA_check_key(IntPtr rsa);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BIO_METHOD
	    {
	        public int type;
	        public IntPtr name;
	        public IntPtr bwrite;
	        public IntPtr bread;
	        public IntPtr bputs;
	        public IntPtr bgets;
	        public IntPtr ctrl;
	        public IntPtr create;
	        public IntPtr destroy;
            public IntPtr callback_ctrl;
	    }


        [StructLayout(LayoutKind.Sequential)]
        private struct BN_GENCB {
            public int ver;
            public IntPtr arg;
            public IntPtr cb;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EVP_CIPHER
        {
        	public int nid;
	        public int block_size;
	        public int key_len;
	        public int iv_len;
	        public uint flags;
            public IntPtr init; // init key
            public IntPtr do_chiper; //encrypt/decrypt data
            public IntPtr cleanup; // cleanup ctx
            public int ctx_size; // size of ctx->cipher_data
	        public IntPtr set_asn1_parameters;
            public IntPtr get_asn1_parameters;
	        public IntPtr ctrl;
	        public IntPtr app_data;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BIO
        {
            public IntPtr method;
	        public IntPtr callback;
	        public IntPtr cb_arg;
	        public int init;
	        public int shutdown;
	        public int flags;
	        public int retry_reason;
	        public int num;
	        public IntPtr ptr;
	        public IntPtr next_bio;
	        public IntPtr prev_bio;
	        public int references;
	        public uint num_read;
	        public uint num_write;

	        CRYPTO_EX_DATA ex_data;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CRYPTO_EX_DATA
        {
        	public IntPtr sk;
	        public int dummy;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BIGNUM
        {
	        public IntPtr d;	// Pointer to an array of 'BN_BITS2' bit chunks.
	        public int top;	    // Index of last used d +1.
	                            // The next are internal book keeping for bn_expand.
	        public int dmax;	// Size of the d array.
	        public int neg;	    // one if the number is negative
	        public int flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RSA
        {
	        /* The first parameter is used to pickup errors where
	         * this is passed instead of aEVP_PKEY, it is set to 0 */
	        public int pad;
	        public int version;
	        public IntPtr meth; /* functional reference if 'meth' is ENGINE-provided */
	        public IntPtr engine;
            public IntPtr n;
            public IntPtr e;
            public IntPtr d;
            public IntPtr p;
            public IntPtr q;
            public IntPtr dmp1;
            public IntPtr dmq1;
            public IntPtr iqmp;
	        CRYPTO_EX_DATA ex_data;
	        public int references;
	        public int flags;

            public IntPtr method_mod_n;
	        public IntPtr method_mod_p;
	        public IntPtr method_mod_q;

	        /* all BIGNUM values are actually in the following data, if it is not
	         * NULL */
	        public IntPtr bignum_data;
            public IntPtr blinding;
            public IntPtr mt_blinding;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PW_CB_DATA
        {
        	IntPtr password;
	        IntPtr prompt_info;
        }
        

        [StructLayout(LayoutKind.Sequential)]
        private struct BigNum
        {
            public readonly IntPtr D;
            public readonly int Top;
            public readonly int DMax;
            public readonly int Neg;
            public readonly int Flags;
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct BnGenCb
        {
            public readonly uint Ver;
            public readonly IntPtr Arg;
            public readonly IntPtr Cb;
        }
    }
}
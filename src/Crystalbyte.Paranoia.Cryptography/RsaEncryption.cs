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
            _handle = NativeMethods.RsaNew();
        }

        public void GenerateRSA()
        {
           /* 
            BN_GENCB cb;
	        int ret=1;
	        int non_fips_allow = 0;
	        int i,num=DEFBITS; // 1024
	        long l;
	        const EVP_CIPHER *enc=NULL;
	        unsigned long f4=RSA_F4;
	        char *outfile=NULL;
	        char *passargout = NULL, *passout = NULL;
	        char *inrand=NULL;
	        BIO *out=NULL;
	        BIGNUM *bn = BN_new();
	        RSA *rsa = NULL;

	        if(!bn) goto err;
	        BN_GENCB_set(&cb, genrsa_cb, bio_err);
            
	        rsa = RSA_new();

	        if (!rsa)
		        goto err;

	        if(!BN_set_word(bn, f4) || !RSA_generate_key_ex(rsa, num, bn, &cb))
		        goto err;

	        app_RAND_write_file(NULL, bio_err);

	        PW_CB_DATA cb_data;
	        cb_data.password = passout;
	        cb_data.prompt_info = outfile;
	        if (!PEM_write_bio_RSAPrivateKey(out,rsa,enc,NULL,0,
		        (pem_password_cb *)password_callback,&cb_data))
		        goto err;
	        }

	        ret=0;
        err:
	        if (bn) BN_free(bn);
	        if (rsa) RSA_free(rsa);
	        if (out) BIO_free_all(out);
	        if(passout) OPENSSL_free(passout);
	        if (ret != 0)
		        ERR_print_errors(bio_err);
	        apps_shutdown();
	        OPENSSL_EXIT(ret);
	        }
     */

            /*
            num=4096; //keysize
            uint f4 = 0x10001;
            var enc = NativeMethods.EVP_aes_256_cbc();
            var bio_out = BIO_new(BIO_s_mem());
	        var bn = NativeMethods.BN_new();      
            var rsa = NativeMethods.RsaNew();
            NativeMethods.BN_set_word(bn, f4);
            NativeMethods.RsaGenerateKeyEx(rsa,num,bn,cb);
            var cb_data = new PW_CB_DATA();
            NativeMethods.PEM_write_bio_RSAPrivateKey(bio_out,rsa,enc,IntPtr.Zero,0,IntPtr.Zero,cb_data);       
             */
        }

        public String Encrypt()
        {
            return "";
            //RSA_public_encrypt(int flen, IntPtr from, IntPtr to, IntPtr rsa, int padding);
        }

        protected override void DisposeNative()
        {
            base.DisposeNative();

            if (_handle != IntPtr.Zero)
            {
                NativeMethods.RsaFree(_handle);
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


            // EVP

            [DllImport(OpenSsl.Library, EntryPoint = "EVP_aes_256_cbc")]
            public static extern IntPtr EVP_aes_256_cbc();

            [DllImport(OpenSsl.Library, EntryPoint = "EVP_PKEY_assign_RSA")]
            public static extern int EVP_PKEY_assign_RSA(IntPtr pkey, IntPtr keypair);

            [DllImport(OpenSsl.Library, EntryPoint = "EVP_PKEY_new")]
            public static extern IntPtr EVP_PKEY_new();

            // PEM

            [DllImport(OpenSsl.Library, EntryPoint = "PEM_write_bio_PKCS8PrivateKey")]
            public static extern int PEM_write_bio_PKCS8PrivateKey(IntPtr bp, IntPtr x, IntPtr enc, IntPtr kstr, int klen, IntPtr cb, IntPtr u);

            [DllImport(OpenSsl.Library, EntryPoint = "PEM_write_bio_RSAPrivateKey")]
            public static extern int PEM_write_bio_RSAPrivateKey(IntPtr bio_out, IntPtr rsa, IntPtr enc, IntPtr kstr, int klen, IntPtr password_callback, IntPtr cb_data);

            // BN

            [DllImport(OpenSsl.Library, EntryPoint = "BN_new")]
            public static extern IntPtr BN_new();

            [DllImport(OpenSsl.Library, EntryPoint = "BN_free")]
            public static extern void BN_free(IntPtr bignum);

            [DllImport(OpenSsl.Library, EntryPoint = "BN_set_word")]
            public static extern void BN_set_word(IntPtr bignum, ulong w);

            // RSA

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_new")]
            public static extern IntPtr RsaNew();

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_free")]
            public static extern void RsaFree(IntPtr handle);

            [DllImport(OpenSsl.Library, EntryPoint = "RSA_generate_key_ex")]
            public static extern IntPtr RsaGenerateKeyEx(IntPtr rsa, int bits, IntPtr e, IntPtr cb);

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
            /// <summary>
            ///   Pointer to an array of 'BN_BITS2' bit chunks.
            /// </summary>
            public readonly IntPtr D;

            /// <summary>
            ///   Index of last used d +1.
            /// </summary>
            public readonly int Top;

            /// <summary>
            ///   The next are internal book keeping for bn_expand.
            /// </summary>
            public readonly int DMax;

            public readonly int Neg;
            public readonly int Flags;
        }

        /// <summary>
        ///   Slow generatio
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct BnGenCb
        {
            /// <summary>
            ///   To handle binary (in)compatibility.
            /// </summary>
            public readonly uint Ver;

            /// <summary>
            ///   Callback specific data;
            /// </summary>
            public readonly IntPtr Arg;

            /// <summary>
            ///   Callback (new style).
            /// </summary>
            public readonly IntPtr Cb;
        }
    }
}
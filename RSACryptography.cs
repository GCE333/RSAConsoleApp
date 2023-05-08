using System.Numerics;
using System.Security.Cryptography;

namespace RSAConsoleApp
{
    internal class RSACryptography
    {
        /// <summary>
        ///     Encrypts a message using the RSA algorithm.
        /// </summary>
        /// <param name="rsaKey">RSA public key</param>
        /// <param name="M">Message to be encrypted</param>
        /// <param name="usePadding">true to use PKCS #1 v1.5 padding, false to encrypt without padding</param>
        /// <returns>Ciphertext, an octet string</returns>
        static public byte[] Encrypt(RSAKeyComponents rsaKey, byte[] M, bool usePadding)
        {
            if (usePadding)
            {
                return RSAEncryptPkcs1(rsaKey.N, rsaKey.E, M);
            }
            else
            {
                BigInteger m = new BigInteger(M, true, true);
                BigInteger c = RSAEP(rsaKey.N, rsaKey.E, m);
                return c.ToByteArray(true, true);
            }
        }

        /// <summary>
        ///     Decrypts a message using the RSA algorithm.
        /// </summary>
        /// <param name="rsaKey">RSA private key</param>
        /// <param name="C">Ciphertext to be decrypted</param>
        /// <param name="usePadding">true to use PKCS #1 v1.5 padding, false to encrypt without padding</param>
        /// <returns>Decrypted message, an octet string</returns>
        static public byte[] Decrypt(RSAKeyComponents rsaKey, byte[] C, bool usePadding)
        {
            if (usePadding)
            {
                return RSADecryptPkcs1(rsaKey.N, rsaKey.D, C);
            }
            else
            {
                BigInteger c = new BigInteger(C, true, true);
                BigInteger m = RSADP(rsaKey.N, rsaKey.D, c);
                return m.ToByteArray(true, true);
            }
        }

        /// <summary>
        ///     Encrypts data using PKCS #1 v1.5 padding.
        /// </summary>
        /// <param name="n">RSA modulus n</param>
        /// <param name="e">RSA public exponent e</param>
        /// <param name="M">Message to be encrypted</param>
        /// <returns>Ciphertext, an octet string</returns>
        static private byte[] RSAEncryptPkcs1(BigInteger n, BigInteger e, byte[] M)
        {
            // k denotes the length in octets of the modulus n
            int k = n.GetByteCount(true);

            int mLen = M.Length;
            if (mLen > k - 11) throw new Exception("message too long");

            /* Generate an octet string PS of length k - mLen - 3 consisting
            of pseudo-randomly generated nonzero octets.  The length of PS
            will be at least eight octets */
            byte[] PS = new byte[k - mLen - 3];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetNonZeroBytes(PS);

            /* Concatenate PS, the message M, and other padding to form an
            encoded message EM of length k octets as
            EM = 0x00 || 0x02 || PS || 0x00 || M */
            byte[] EM = new byte[] { 0x00, 0x02 }.Concat(PS).Concat(new byte[] { 0x00 }).Concat(M).ToArray();

            /* Convert the encoded message EM to an integer message
            representative m */
            BigInteger m = new BigInteger(new ReadOnlySpan<byte>(EM), true, true);

            /* Apply the RSAEP encryption primitive to the RSA
            public key (n, e) and the message representative m to produce
            an integer ciphertext representative c */
            BigInteger c = RSAEP (n, e, m);

            /* Convert the ciphertext representative c to a ciphertext C of
            length k octets */
            byte[] C = I2OSP(c, k);
            return C;
        }

        /// <summary>
        ///     Decrypts data with PKCS #1 v1.5 padding.
        /// </summary>
        /// <param name="n">RSA modulus n</param>
        /// <param name="d">RSA private exponent d</param>
        /// <param name="C">Ciphertext to be decrypted</param>
        /// <returns>Decrypted message, an octet string</returns>
        static private byte[] RSADecryptPkcs1(BigInteger n, BigInteger d, byte[] C)
        {
            // k is the length in octets of the RSA modulus n
            int k = n.GetByteCount(true);

            if (C.Length != k || k < 11) throw new Exception("decryption error");

            /* Convert the ciphertext C to an integer ciphertext
            representative c */
            BigInteger c = new BigInteger(new ReadOnlySpan<byte>(C), true, true);

            /* Apply the RSADP decryption primitive to the RSA
            private key (n, d) and the ciphertext representative c to
            produce an integer message representative m */
            BigInteger m = RSADP(n, d, c);

            /* Convert the message representative m to an encoded message EM
            of length k octets */
            byte[] EM = I2OSP(m, k);

            /* Separate the encoded message EM into an octet string PS
            consisting of nonzero octets and a message M as
            EM = 0x00 || 0x02 || PS || 0x00 || M. */
            int i = 2;
            while (i < k)
            {
                if (EM[i] == 0x00) break;
                else i++;
            }
            // the first octet of EM does not have hexadecimal value 0x00
            if (EM[0] != 0x00) throw new Exception("decryption error");

            // the second octet of EM does not have hexadecimal value 0x02
            if (EM[1] != 0x02) throw new Exception("decryption error");

            // there is no octet with hexadecimal value 0x00 to separate PS from M
            if (EM[i] != 0x00) throw new Exception("decryption error");

            // the length of PS is less than 8 octets
            if (i - 2 < 8) throw new Exception("decryption error");

            int mLen = k - i - 1;
            byte[] M = new byte[mLen];
            Array.Copy(EM, i + 1, M, 0, mLen);
            return M;
        }

        /// <summary>
        ///     RSA encryption primitive
        /// </summary>
        /// <param name="n">RSA modulus n</param>
        /// <param name="e">RSA public exponent e</param>
        /// <param name="m">Message representative, an integer between 0 and n - 1</param>
        /// <returns>Ciphertext representative, an integer between 0 and n - 1</returns>
        static private BigInteger RSAEP(BigInteger n, BigInteger e, BigInteger m)
        {
            if (!(m > 0 && m < n - 1)) throw new Exception("message representative out of range");
            BigInteger c = BigInteger.ModPow(m, e, n);
            return c;
        }

        /// <summary>
        ///     RSA decryption primitive
        /// </summary>
        /// <param name="n">RSA modulus n</param>
        /// <param name="d">RSA private exponent d</param>
        /// <param name="c">Ciphertext representative, an integer between 0 and n - 1</param>
        /// <returns>Message representative, an integer between 0 and n - 1</returns>
        static private BigInteger RSADP(BigInteger n, BigInteger d, BigInteger c)
        {
            if (!(c > 0 && c < n - 1)) throw new Exception("ciphertext representative out of range");
            BigInteger m = BigInteger.ModPow(c, d, n);
            return m;
        }

        /// <summary>
        ///     Integer-to-Octet-String primitive.
        ///     Converts a nonnegative integer to an octet string of a
        ///     specified length.
        /// </summary>
        /// <param name="x">Nonnegative integer to be converted</param>
        /// <param name="xLen">Intended length of the resulting octet string</param>
        /// <returns>Corresponding octet string of length xLen</returns>
        static private byte[] I2OSP(BigInteger x, int xLen)
        {
            if (x >= BigInteger.Pow(256, xLen)) throw new Exception("integer too large");
            byte[] byteArray = x.ToByteArray(true, true);
            byte[] X = new byte[xLen];
            Array.Copy(byteArray, 0, X, xLen - byteArray.Length, byteArray.Length);
            return X;
        }
    }
}
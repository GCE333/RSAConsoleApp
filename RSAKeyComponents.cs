using System.Numerics;

namespace RSAConsoleApp
{
    internal class RSAKeyComponents
    {
        /// <summary>
        ///     Represents the Modulus parameter for the RSA algorithm.
        /// </summary>
        public BigInteger N { get; }

        /// <summary>
        ///     Represents the public exponent parameter for the RSA algorithm.
        /// </summary>
        public BigInteger E { get; }

        /// <summary>
        ///     Represents the private exponent parameter for the RSA algorithm.
        /// </summary>
        public BigInteger D { get; }

        /// <summary>
        ///     Indicates whether only public components of the key are set.
        /// </summary>
        public bool PublicOnly { get; }

        /// <summary>
        ///     Indicates whether the key components are empty.
        /// </summary>
        public bool IsEmpty { get; }

        public RSAKeyComponents()
        {
            IsEmpty = true;
        }

        public RSAKeyComponents(BigInteger modulus, BigInteger e, BigInteger d)
        {
            N = modulus;
            E = e;
            D = d;
            IsEmpty = false;
        }

        public RSAKeyComponents(BigInteger modulus, BigInteger e)
        {
            N = modulus;
            E = e;
            PublicOnly = true;
            IsEmpty = false;
        }
    }
}
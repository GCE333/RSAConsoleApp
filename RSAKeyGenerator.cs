using System.Numerics;
using System.Security.Cryptography;
using System.Diagnostics;

namespace RSAConsoleApp
{
    internal class RSAKeyGenerator
    {
        /// <summary>
        ///     Performs multiple rounds of the Miller-Rabin
        ///     test with randomly chosen bases
        /// </summary>
        /// <param name="n">An integer to be tested for primality</param>
        /// <param name="k">Iterations of the test to run</param>
        /// <returns>
        ///     true if the number passes all rounds
        ///     of the test, and false otherwise
        /// </returns>
        static bool IsProbablyPrime(BigInteger n, int k)
        {
            if (n < 2) return false;
            if (n == 2 || n == 3) return true;
            if (n.IsEven) return false;
            
            // Find s and d such that n-1 = d * 2^s
            BigInteger d = n - 1;
            int s = 0;
            while (d.IsEven)
            {
                d /= 2;
                s++;
            }

            byte[] bytes = new byte[n.GetByteCount(true)];
            Random random = new Random();
            
            // Repeat k times
            for (int i = 0; i < k; i++)
            {
                // Choose a random a between 2 and n-2
                BigInteger a;
                do
                {
                    random.NextBytes(bytes);
                    a = new BigInteger(bytes, true, true);
                } while (a < 2 || a > n - 2);
                
                // Compute x = a^d % n
                BigInteger x = BigInteger.ModPow(a, d, n);
                // If x == 1 or x == n-1, n might be prime, so continue with the loop
                if (x == 1 || x == n - 1) continue;
                
                // Square x s-1 times modulo n until we get x = n-1, or x == 1
                for (int j = 1; j < s; j++)
                {
                    x = BigInteger.ModPow(x, 2, n);
                    if (x == n - 1) break;
                    if (x == 1) return false;
                }
                if (x != n - 1) return false;
            }

            return true;
        }
        
        /// <summary>
        ///     Generates a random odd integer of the given bit length
        /// </summary>
        static BigInteger GenerateOddNumber(int bitLength)
        {
            byte[] bytes = RandomNumberGenerator.GetBytes(bitLength / 8);

            BigInteger number = new BigInteger(bytes, true, true);
            number |= BigInteger.One;
            number |= (BigInteger.One << (bitLength - 1));

            return number;
        }

        static BigInteger ModularInverse(BigInteger a, BigInteger n)
        {
            BigInteger t = 0, newT = 1;
            BigInteger r = n, newR = a;

            while (newR != 0)
            {
                BigInteger quotient = r / newR;

                BigInteger tempT = newT;
                newT = t - quotient * newT;
                t = tempT;

                BigInteger tempR = newR;
                newR = r - quotient * newR;
                r = tempR;
            }

            if (r > 1) throw new ArgumentException("a is not invertible");

            if (t < 0) t += n;

            return t;
        }

        static BigInteger CalculateGCD(BigInteger a, BigInteger b)
        {
            while (b != 0)
            {
                var temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        static public RSAKeyComponents GenerateKeys(int keySize)
        {
            BigInteger p, q, n, phiN, e;
            int bitLength = keySize / 2;
            BigInteger gcd;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            do
            {
                do p = GenerateOddNumber(bitLength); while (!IsProbablyPrime(p, 20));
                do q = GenerateOddNumber(bitLength); while (!IsProbablyPrime(q, 20));

                // Calculate n = pq
                n = p * q;

                // Calculate phi(n) = (p - 1)(q - 1)
                phiN = (p - 1) * (q - 1);

                // Choose a small odd integer e such that gcd(e, phi(n)) == 1
                e = new BigInteger(65537); // Commonly used value for e
                gcd = CalculateGCD(e, phiN);
            }
            while (gcd != 1);

            // Calculate the modular inverse of e mod phi(n), which is d
            BigInteger d = ModularInverse(e, phiN);
            stopwatch.Stop();

            Console.WriteLine("p: " + p);
            Console.WriteLine("q: " + q);
            Console.WriteLine("n: " + n);
            Console.WriteLine("phiN: " + phiN);
            Console.WriteLine("e: " + e);
            Console.WriteLine("d: " + d);
            Console.WriteLine("RSA keys generated in {0} ms", stopwatch.Elapsed.TotalMilliseconds);
            return new RSAKeyComponents(n, e, d);
        }
    }
}